import * as cdk from 'aws-cdk-lib';
import * as s3 from 'aws-cdk-lib/aws-s3';
import * as cloudfront from 'aws-cdk-lib/aws-cloudfront';
import * as origins from 'aws-cdk-lib/aws-cloudfront-origins';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as s3deploy from 'aws-cdk-lib/aws-s3-deployment';
import { Construct } from 'constructs';

export class SapCloneStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // 🌐 Frontend S3 Bucket for Static Website
    const frontendBucket = new s3.Bucket(this, 'SapCloneFrontendBucket', {
      bucketName: `sap-clone-frontend-${Date.now().toString().slice(-8)}`,
      publicReadAccess: false,
      blockPublicAccess: s3.BlockPublicAccess.BLOCK_ALL,
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      autoDeleteObjects: true,
      websiteIndexDocument: 'index.html',
      websiteErrorDocument: 'index.html',
    });

    // 🚀 CloudFront Distribution
    const distribution = new cloudfront.Distribution(this, 'SapCloneDistribution', {
      defaultBehavior: {
        origin: new origins.S3Origin(frontendBucket),
        viewerProtocolPolicy: cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
        cachePolicy: cloudfront.CachePolicy.CACHING_OPTIMIZED,
        originRequestPolicy: cloudfront.OriginRequestPolicy.CORS_S3_ORIGIN,
      },
      defaultRootObject: 'index.html',
      errorResponses: [
        {
          httpStatus: 404,
          responseHttpStatus: 200,
          responsePagePath: '/index.html',
        },
        {
          httpStatus: 403,
          responseHttpStatus: 200,
          responsePagePath: '/index.html',
        },
      ],
      comment: 'sap-clone-distribution',
    });

    // 📦 Deployment S3 Bucket
    const deploymentBucket = new s3.Bucket(this, 'SapCloneDeploymentBucket', {
      bucketName: 'sap-clone-deployments',
      publicReadAccess: false,
      blockPublicAccess: s3.BlockPublicAccess.BLOCK_ALL,
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      autoDeleteObjects: true,
      versioned: true,
      lifecycleRules: [
        {
          id: 'DeleteOldVersions',
          enabled: true,
          noncurrentVersionExpiration: cdk.Duration.days(30),
        },
      ],
    });

    // 🔒 VPC for Secure Networking
    const vpc = new ec2.Vpc(this, 'SapCloneVpc', {
      maxAzs: 2,
      natGateways: 0, // Free tier doesn't include NAT gateways
      subnetConfiguration: [
        {
          cidrMask: 24,
          name: 'Public',
          subnetType: ec2.SubnetType.PUBLIC,
        },
      ],
    });

    // 🔐 Security Group for EC2
    const webSecurityGroup = new ec2.SecurityGroup(this, 'SapCloneWebSG', {
      vpc,
      description: 'Security group for SAP Clone API server',
      allowAllOutbound: true,
    });

    // Allow HTTP traffic
    webSecurityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(80),
      'Allow HTTP traffic'
    );

    // Allow HTTPS traffic
    webSecurityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(443),
      'Allow HTTPS traffic'
    );

    // Allow API traffic (port 5000)
    webSecurityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(5000),
      'Allow API traffic'
    );

    // Allow SSH for maintenance (restrict this in production)
    webSecurityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(22),
      'Allow SSH access'
    );

    // 🎯 IAM Role for EC2 Instance
    const ec2Role = new iam.Role(this, 'SapCloneEC2Role', {
      assumedBy: new iam.ServicePrincipal('ec2.amazonaws.com'),
      managedPolicies: [
        iam.ManagedPolicy.fromAwsManagedPolicyName('AmazonSSMManagedInstanceCore'),
        iam.ManagedPolicy.fromAwsManagedPolicyName('CloudWatchAgentServerPolicy'),
      ],
    });

    // Allow EC2 to access deployment bucket
    deploymentBucket.grantRead(ec2Role);

    // Allow EC2 to access CloudWatch
    ec2Role.addToPolicy(
      new iam.PolicyStatement({
        effect: iam.Effect.ALLOW,
        actions: [
          'logs:CreateLogGroup',
          'logs:CreateLogStream',
          'logs:PutLogEvents',
        ],
        resources: ['*'],
      })
    );

    // 🖥️ EC2 Instance for API
    const instance = new ec2.Instance(this, 'SapCloneAPIInstance', {
      vpc,
      instanceType: ec2.InstanceType.of(ec2.InstanceClass.T2, ec2.InstanceSize.MICRO),
      machineImage: ec2.MachineImage.latestAmazonLinux2023(),
      securityGroup: webSecurityGroup,
      role: ec2Role,
      keyName: undefined, // We'll use SSM for access instead of SSH keys
      userData: ec2.UserData.forLinux(),
      blockDevices: [
        {
          deviceName: '/dev/xvda',
          volume: ec2.BlockDeviceVolume.ebs(20), // 20GB free tier eligible
        },
      ],
    });

    // Tag the instance for easy identification
    cdk.Tags.of(instance).add('Name', 'sap-clone-api');

    // User data script for initial setup
    instance.userData.addCommands(
      // Update system
      'sudo dnf update -y',
      
      // Install .NET 9
      'sudo dnf install -y dotnet-sdk-9.0',
      
      // Install Docker
      'sudo dnf install -y docker',
      'sudo systemctl start docker',
      'sudo systemctl enable docker',
      'sudo usermod -a -G docker ec2-user',
      
      // Install PostgreSQL client
      'sudo dnf install -y postgresql15',
      
      // Install AWS CLI v2
      'curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"',
      'unzip awscliv2.zip',
      'sudo ./aws/install',
      
      // Create application directory
      'sudo mkdir -p /opt/sap-api',
      'sudo chown ec2-user:ec2-user /opt/sap-api',
      
      // Create systemd service file
      'sudo tee /etc/systemd/system/sap-api.service > /dev/null <<EOF',
      '[Unit]',
      'Description=SAP Clone API',
      'After=network.target',
      '',
      '[Service]',
      'Type=notify',
      'ExecStart=/opt/sap-api/SAP.API',
      'Restart=always',
      'RestartSec=5',
      'KillSignal=SIGTERM',
      'User=ec2-user',
      'WorkingDirectory=/opt/sap-api',
      'Environment=ASPNETCORE_ENVIRONMENT=Production',
      'Environment=ASPNETCORE_URLS=http://0.0.0.0:5000',
      '',
      '[Install]',
      'WantedBy=multi-user.target',
      'EOF',
      
      // Enable the service
      'sudo systemctl daemon-reload',
      'sudo systemctl enable sap-api',
    );

    // 🎯 OIDC Role for GitHub Actions
    const githubOidcProvider = new iam.OpenIdConnectProvider(this, 'GitHubOIDC', {
      url: 'https://token.actions.githubusercontent.com',
      clientIds: ['sts.amazonaws.com'],
      thumbprints: ['6938fd4d98bab03faadb97b34396831e3780aea1'],
    });

    const githubActionsRole = new iam.Role(this, 'GitHubActionsRole', {
      assumedBy: new iam.WebIdentityPrincipal(
        githubOidcProvider.openIdConnectProviderArn,
        {
          StringEquals: {
            'token.actions.githubusercontent.com:aud': 'sts.amazonaws.com',
          },
          StringLike: {
            'token.actions.githubusercontent.com:sub': 'repo:*:ref:refs/heads/main',
          },
        }
      ),
      managedPolicies: [
        iam.ManagedPolicy.fromAwsManagedPolicyName('ReadOnlyAccess'),
      ],
    });

    // Grant permissions for deployment
    frontendBucket.grantReadWrite(githubActionsRole);
    deploymentBucket.grantReadWrite(githubActionsRole);

    githubActionsRole.addToPolicy(
      new iam.PolicyStatement({
        effect: iam.Effect.ALLOW,
        actions: [
          'cloudfront:CreateInvalidation',
          'cloudfront:ListDistributions',
          'ec2:DescribeInstances',
          'ssm:SendCommand',
          'ssm:GetCommandInvocation',
        ],
        resources: ['*'],
      })
    );

    // 📊 Outputs
    new cdk.CfnOutput(this, 'FrontendBucketName', {
      value: frontendBucket.bucketName,
      description: 'S3 bucket name for frontend deployment',
    });

    new cdk.CfnOutput(this, 'CloudFrontURL', {
      value: `https://${distribution.distributionDomainName}`,
      description: 'CloudFront distribution URL',
    });

    new cdk.CfnOutput(this, 'EC2InstanceId', {
      value: instance.instanceId,
      description: 'EC2 instance ID for API server',
    });

    new cdk.CfnOutput(this, 'EC2PublicIP', {
      value: instance.instancePublicIp,
      description: 'EC2 instance public IP',
    });

    new cdk.CfnOutput(this, 'GitHubActionsRoleArn', {
      value: githubActionsRole.roleArn,
      description: 'GitHub Actions OIDC role ARN',
    });

    new cdk.CfnOutput(this, 'DeploymentBucketName', {
      value: deploymentBucket.bucketName,
      description: 'S3 bucket for deployment artifacts',
    });
  }
} 