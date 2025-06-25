# 🛠️ AWS CLI Setup & Management Guide

## ✅ **Yes! AWS CLI is Essential**

AWS CLI is a powerful tool that lets you manage all AWS services from the command line. Perfect for automating deployments, managing resources, and integrating with our CI/CD pipeline.

## 🚀 **Installation Guide**

### **Windows Installation**
```powershell
# Method 1: Direct installer (Recommended)
# Download from: https://awscli.amazonaws.com/AWSCLIV2.msi

# Method 2: Using Chocolatey
choco install awscli

# Method 3: Using Scoop
scoop install aws

# Verify installation
aws --version
```

### **Configuration**
```bash
# Configure AWS credentials
aws configure

# You'll be prompted for:
# AWS Access Key ID: [Your access key]
# AWS Secret Access Key: [Your secret key]
# Default region name: us-east-1
# Default output format: json
```

### **Alternative: Environment Variables**
```powershell
# Set environment variables (more secure for CI/CD)
$env:AWS_ACCESS_KEY_ID="your-access-key"
$env:AWS_SECRET_ACCESS_KEY="your-secret-key"
$env:AWS_DEFAULT_REGION="us-east-1"
```

## 📋 **Essential AWS CLI Commands for SAP Clone**

### **🔍 Account & Identity**
```bash
# Check your AWS identity
aws sts get-caller-identity

# List available regions
aws ec2 describe-regions --output table

# Check service quotas
aws service-quotas list-service-quotas --service-code ec2
```

### **🖥️ EC2 Management**
```bash
# List instances
aws ec2 describe-instances --output table

# Get instance status
aws ec2 describe-instance-status --instance-ids i-1234567890abcdef0

# Start/Stop instance
aws ec2 start-instances --instance-ids i-1234567890abcdef0
aws ec2 stop-instances --instance-ids i-1234567890abcdef0

# Get instance public IP
aws ec2 describe-instances \
  --instance-ids i-1234567890abcdef0 \
  --query 'Reservations[0].Instances[0].PublicIpAddress' \
  --output text
```

### **🗂️ S3 Operations**
```bash
# Create S3 bucket
aws s3 mb s3://your-sap-clone-frontend

# Upload files (sync React build)
aws s3 sync ./src/Web/sap-web/dist s3://your-sap-clone-frontend --delete

# Enable static website hosting
aws s3 website s3://your-sap-clone-frontend \
  --index-document index.html \
  --error-document index.html

# Set bucket policy for public access
aws s3api put-bucket-policy \
  --bucket your-sap-clone-frontend \
  --policy file://bucket-policy.json
```

### **🌐 CloudFront Management**
```bash
# List distributions
aws cloudfront list-distributions --output table

# Create invalidation (clear cache)
aws cloudfront create-invalidation \
  --distribution-id E1234567890ABC \
  --paths "/*"

# Get distribution status
aws cloudfront get-distribution --id E1234567890ABC
```

### **🗄️ RDS Operations**
```bash
# List RDS instances
aws rds describe-db-instances --output table

# Check RDS status
aws rds describe-db-instances \
  --db-instance-identifier sap-clone-db \
  --query 'DBInstances[0].DBInstanceStatus'

# Create snapshot
aws rds create-db-snapshot \
  --db-instance-identifier sap-clone-db \
  --db-snapshot-identifier sap-clone-backup-$(date +%Y%m%d)
```

### **📊 CloudWatch Monitoring**
```bash
# Get EC2 CPU metrics
aws cloudwatch get-metric-statistics \
  --namespace AWS/EC2 \
  --metric-name CPUUtilization \
  --dimensions Name=InstanceId,Value=i-1234567890abcdef0 \
  --statistics Average \
  --start-time 2024-01-01T00:00:00Z \
  --end-time 2024-01-02T00:00:00Z \
  --period 3600

# Create custom dashboard
aws cloudwatch put-dashboard \
  --dashboard-name "SAP-Clone-Monitoring" \
  --dashboard-body file://dashboard.json
```

## 🚀 **Complete Deployment Script**

I've created a comprehensive PowerShell deployment script: **`deploy-aws.ps1`**

### **Key Features:**
- ✅ **Infrastructure Creation** - Sets up S3, CloudFront automatically
- ✅ **Frontend Deployment** - Builds React + uploads to S3 + invalidates CDN
- ✅ **Backend Deployment** - Publishes .NET API + creates deployment package
- ✅ **AWS Resource Management** - Start/stop instances, get IPs
- ✅ **Error Handling** - Comprehensive checks and validation

### **Usage Examples:**
```powershell
# Show help
.\deploy-aws.ps1 -Help

# Create AWS infrastructure
.\deploy-aws.ps1 -CreateInfrastructure -S3Bucket "my-sap-clone"

# Deploy frontend only
.\deploy-aws.ps1 -S3Bucket "my-sap-clone" -DistributionId "E123ABC" -SkipBackend

# Deploy backend only  
.\deploy-aws.ps1 -EC2InstanceId "i-1234567890abcdef0" -SkipFrontend

# Full deployment
.\deploy-aws.ps1 -S3Bucket "my-sap-clone" -DistributionId "E123ABC" -EC2InstanceId "i-1234567890abcdef0"
```

## 🔧 **Free Tier Resource Creation**

### **1. Create EC2 Instance (Free Tier)**
```bash
# Launch t2.micro instance
aws ec2 run-instances \
  --image-id ami-0c02fb55956c7d316 \
  --instance-type t2.micro \
  --key-name your-key-pair \
  --security-group-ids sg-12345678 \
  --subnet-id subnet-12345678 \
  --tag-specifications 'ResourceType=instance,Tags=[{Key=Name,Value=SAP-Clone-API}]'
```

### **2. Create RDS Instance (Free Tier)**
```bash
# Create PostgreSQL t2.micro
aws rds create-db-instance \
  --db-instance-identifier sap-clone-db \
  --db-instance-class db.t2.micro \
  --engine postgres \
  --engine-version 15.7 \
  --master-username sapuser \
  --master-user-password your-secure-password \
  --allocated-storage 20 \
  --storage-type gp2 \
  --vpc-security-group-ids sg-12345678
```

### **3. Create Security Groups**
```bash
# API security group
aws ec2 create-security-group \
  --group-name sap-clone-api \
  --description "SAP Clone API security group"

# Add rules
aws ec2 authorize-security-group-ingress \
  --group-name sap-clone-api \
  --protocol tcp \
  --port 22 \
  --cidr 0.0.0.0/0

aws ec2 authorize-security-group-ingress \
  --group-name sap-clone-api \
  --protocol tcp \
  --port 5000 \
  --cidr 0.0.0.0/0
```

## 📊 **Monitoring Commands**

### **Cost Tracking**
```bash
# Check current month costs
aws ce get-cost-and-usage \
  --time-period Start=2024-01-01,End=2024-01-31 \
  --granularity MONTHLY \
  --metrics BlendedCost

# Free tier usage
aws support describe-trusted-advisor-checks \
  --language en \
  --query 'checks[?name==`Service Limits`]'
```

### **Resource Monitoring**
```bash
# EC2 status check
aws ec2 describe-instance-status \
  --instance-ids i-1234567890abcdef0

# RDS status
aws rds describe-db-instances \
  --db-instance-identifier sap-clone-db \
  --query 'DBInstances[0].DBInstanceStatus'

# S3 bucket size
aws s3api head-bucket --bucket your-sap-clone-frontend
aws s3 ls s3://your-sap-clone-frontend --recursive --summarize
```

## 🚨 **Free Tier Alerts**

### **Create Billing Alarm**
```bash
# Create SNS topic for alerts
aws sns create-topic --name billing-alerts

# Create CloudWatch alarm
aws cloudwatch put-metric-alarm \
  --alarm-name "Free-Tier-Billing" \
  --alarm-description "Alert when approaching free tier limits" \
  --metric-name EstimatedCharges \
  --namespace AWS/Billing \
  --statistic Maximum \
  --period 86400 \
  --threshold 1 \
  --comparison-operator GreaterThanThreshold \
  --alarm-actions arn:aws:sns:us-east-1:123456789012:billing-alerts
```

## 🎯 **Quick Start Checklist**

### **Setup (One Time)**
- [ ] Install AWS CLI
- [ ] Run `aws configure`
- [ ] Create EC2 key pair
- [ ] Run `.\deploy-aws.ps1 -CreateInfrastructure`
- [ ] Launch EC2 t2.micro instance
- [ ] Create RDS PostgreSQL t2.micro
- [ ] Set up security groups

### **Daily Development**
- [ ] Code changes
- [ ] Run `.\deploy-aws.ps1` for full deployment
- [ ] Test frontend and backend
- [ ] Monitor costs in AWS console

Your AWS CLI setup is now complete and ready for professional development! 🚀 