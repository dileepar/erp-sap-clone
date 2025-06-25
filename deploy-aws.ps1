# SAP Clone AWS Deployment Script
param(
    [string]$S3Bucket = "your-sap-clone-frontend",
    [string]$DistributionId = "",
    [string]$EC2InstanceId = "",
    [string]$Region = "us-east-1",
    [switch]$SkipFrontend,
    [switch]$SkipBackend,
    [switch]$CreateInfrastructure,
    [switch]$Help
)

function Write-Info($Message) {
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Warning($Message) {
    Write-Host "⚠️ $Message" -ForegroundColor Yellow
}

function Write-Error($Message) {
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Show-Help {
    Write-Host "🚀 SAP Clone AWS Deployment Script" -ForegroundColor Blue
    Write-Host "====================================" -ForegroundColor Blue
    Write-Host ""
    Write-Host "Usage: .\deploy-aws.ps1 [options]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Parameters:" -ForegroundColor Yellow
    Write-Host "  -S3Bucket          S3 bucket name for frontend" -ForegroundColor Cyan
    Write-Host "  -DistributionId    CloudFront distribution ID" -ForegroundColor Cyan
    Write-Host "  -EC2InstanceId     EC2 instance ID for backend" -ForegroundColor Cyan
    Write-Host "  -Region            AWS region (default: us-east-1)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Switches:" -ForegroundColor Yellow
    Write-Host "  -SkipFrontend      Skip frontend deployment" -ForegroundColor Cyan
    Write-Host "  -SkipBackend       Skip backend deployment" -ForegroundColor Cyan
    Write-Host "  -CreateInfrastructure  Create AWS resources" -ForegroundColor Cyan
    Write-Host "  -Help              Show this help" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\deploy-aws.ps1 -CreateInfrastructure"
    Write-Host "  .\deploy-aws.ps1 -S3Bucket 'my-sap-app' -DistributionId 'E123ABC'"
    Write-Host "  .\deploy-aws.ps1 -SkipBackend"
}

if ($Help) {
    Show-Help
    exit 0
}

Write-Host "🚀 SAP Clone AWS Deployment" -ForegroundColor Blue
Write-Host "============================" -ForegroundColor Blue

# Check prerequisites
try {
    $awsVersion = aws --version 2>$null
    Write-Info "AWS CLI: $awsVersion"
} catch {
    Write-Error "AWS CLI not found! Please install it first."
    Write-Host "Download from: https://awscli.amazonaws.com/AWSCLIV2.msi"
    exit 1
}

# Verify AWS credentials
try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    Write-Info "Authenticated as: $($identity.Arn)"
} catch {
    Write-Error "AWS credentials not configured!"
    Write-Host "Run: aws configure"
    exit 1
}

# Create AWS infrastructure if requested
if ($CreateInfrastructure) {
    Write-Host "🏗️ Creating AWS Infrastructure..." -ForegroundColor Cyan
    
    # Create S3 bucket
    Write-Info "Creating S3 bucket: $S3Bucket"
    try {
        aws s3 mb s3://$S3Bucket --region $Region
        
        # Configure static website hosting
        aws s3 website s3://$S3Bucket --index-document index.html --error-document index.html
        
        # Create bucket policy for public access
        $bucketPolicy = @"
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "PublicReadGetObject",
      "Effect": "Allow",
      "Principal": "*",
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::$S3Bucket/*"
    }
  ]
}
"@
        $bucketPolicy | Out-File -FilePath "bucket-policy.json" -Encoding UTF8
        aws s3api put-bucket-policy --bucket $S3Bucket --policy file://bucket-policy.json
        Remove-Item "bucket-policy.json"
        
    } catch {
        Write-Warning "S3 bucket might already exist or error occurred"
    }
    
    # Create CloudFront distribution
    Write-Info "Creating CloudFront distribution..."
    $distributionConfig = @"
{
  "CallerReference": "$(Get-Date -Format 'yyyyMMddHHmmss')",
  "Comment": "SAP Clone Frontend CDN",
  "DefaultCacheBehavior": {
    "TargetOriginId": "S3-$S3Bucket",
    "ViewerProtocolPolicy": "redirect-to-https",
    "AllowedMethods": {
      "Quantity": 2,
      "Items": ["GET", "HEAD"],
      "CachedMethods": {
        "Quantity": 2,
        "Items": ["GET", "HEAD"]
      }
    },
    "ForwardedValues": {
      "QueryString": false,
      "Cookies": {"Forward": "none"}
    },
    "TrustedSigners": {
      "Enabled": false,
      "Quantity": 0
    },
    "MinTTL": 0,
    "DefaultTTL": 86400
  },
  "Origins": {
    "Quantity": 1,
    "Items": [
      {
        "Id": "S3-$S3Bucket",
        "DomainName": "$S3Bucket.s3-website-$Region.amazonaws.com",
        "CustomOriginConfig": {
          "HTTPPort": 80,
          "HTTPSPort": 443,
          "OriginProtocolPolicy": "http-only"
        }
      }
    ]
  },
  "Enabled": true,
  "PriceClass": "PriceClass_100"
}
"@
    
    try {
        $distributionConfig | Out-File -FilePath "distribution-config.json" -Encoding UTF8
        $distribution = aws cloudfront create-distribution --distribution-config file://distribution-config.json --output json | ConvertFrom-Json
        $DistributionId = $distribution.Distribution.Id
        Write-Info "CloudFront Distribution ID: $DistributionId"
        Remove-Item "distribution-config.json"
    } catch {
        Write-Warning "CloudFront distribution creation failed"
    }
    
    Write-Info "🏗️ Infrastructure creation completed!"
    Write-Host "Next steps:"
    Write-Host "1. Launch an EC2 t2.micro instance"
    Write-Host "2. Set up RDS PostgreSQL t2.micro"
    Write-Host "3. Configure security groups"
    Write-Host "4. Update this script with your instance ID"
    exit 0
}

# Frontend deployment
if (-not $SkipFrontend) {
    Write-Host "📦 Deploying Frontend..." -ForegroundColor Cyan
    
    # Build frontend
    Push-Location "src\Web\sap-web"
    try {
        Write-Info "Building React app with Bun..."
        bun run build
        
        if (-not (Test-Path "dist")) {
            Write-Error "Build failed - dist folder not found"
            exit 1
        }
        
        Write-Info "Uploading to S3: $S3Bucket"
        aws s3 sync dist s3://$S3Bucket --delete --region $Region
        
        if ($DistributionId) {
            Write-Info "Invalidating CloudFront cache..."
            $invalidation = aws cloudfront create-invalidation --distribution-id $DistributionId --paths "/*" --output json | ConvertFrom-Json
            Write-Info "CloudFront invalidation: $($invalidation.Invalidation.Id)"
        }
        
        Write-Info "✅ Frontend deployed successfully!"
        
    } catch {
        Write-Error "Frontend deployment failed: $($_.Exception.Message)"
        exit 1
    } finally {
        Pop-Location
    }
}

# Backend deployment
if (-not $SkipBackend) {
    Write-Host "🔧 Deploying Backend..." -ForegroundColor Cyan
    
    if (-not $EC2InstanceId) {
        Write-Error "EC2InstanceId parameter required for backend deployment"
        exit 1
    }
    
    # Build and publish API
    Write-Info "Publishing .NET API..."
    dotnet publish src\API\SAP.API\SAP.API.csproj -c Release -o publish\api
    
    if (-not (Test-Path "publish\api")) {
        Write-Error "API publish failed"
        exit 1
    }
    
    # Create deployment package
    Write-Info "Creating deployment package..."
    Compress-Archive -Path "publish\api\*" -DestinationPath "sap-api-deployment.zip" -Force
    
    # Get EC2 instance details
    $instanceInfo = aws ec2 describe-instances --instance-ids $EC2InstanceId --region $Region --output json | ConvertFrom-Json
    $instance = $instanceInfo.Reservations[0].Instances[0]
    $instanceIp = $instance.PublicIpAddress
    $instanceState = $instance.State.Name
    
    if ($instanceState -ne "running") {
        Write-Warning "EC2 instance is not running (state: $instanceState)"
        Write-Host "Starting instance..."
        aws ec2 start-instances --instance-ids $EC2InstanceId --region $Region
        Write-Host "Waiting for instance to start..."
        aws ec2 wait instance-running --instance-ids $EC2InstanceId --region $Region
        
        # Get updated IP
        $instanceInfo = aws ec2 describe-instances --instance-ids $EC2InstanceId --region $Region --output json | ConvertFrom-Json
        $instanceIp = $instanceInfo.Reservations[0].Instances[0].PublicIpAddress
    }
    
    Write-Info "EC2 Instance IP: $instanceIp"
    Write-Info "Deployment package created: sap-api-deployment.zip"
    
    Write-Host ""
    Write-Warning "Manual deployment steps for EC2:"
    Write-Host "1. Upload sap-api-deployment.zip to your EC2 instance:"
    Write-Host "   scp -i your-key.pem sap-api-deployment.zip ubuntu@$instanceIp`:~/"
    Write-Host ""
    Write-Host "2. SSH into the instance and run:"
    Write-Host "   ssh -i your-key.pem ubuntu@$instanceIp"
    Write-Host "   sudo systemctl stop sap-api"
    Write-Host "   sudo rm -rf /opt/sap-api/*"
    Write-Host "   sudo unzip ~/sap-api-deployment.zip -d /opt/sap-api/"
    Write-Host "   sudo chown -R ubuntu:ubuntu /opt/sap-api"
    Write-Host "   sudo systemctl start sap-api"
    Write-Host "   sudo systemctl status sap-api"
    
    Write-Info "✅ Backend deployment package ready!"
}

# Summary
Write-Host ""
Write-Info "🎉 Deployment completed!"

if (-not $SkipFrontend) {
    if ($DistributionId) {
        Write-Host "Frontend URL: https://d$DistributionId.cloudfront.net" -ForegroundColor Yellow
    }
    Write-Host "S3 Website URL: http://$S3Bucket.s3-website-$Region.amazonaws.com" -ForegroundColor Yellow
}

if (-not $SkipBackend -and $EC2InstanceId) {
    $instanceIp = aws ec2 describe-instances --instance-ids $EC2InstanceId --region $Region --query 'Reservations[0].Instances[0].PublicIpAddress' --output text
    Write-Host "API URL: http://$instanceIp:5000" -ForegroundColor Yellow
}

# Cleanup
Remove-Item "publish" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "sap-api-deployment.zip" -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Info "Deployment script completed successfully! 🚀" 