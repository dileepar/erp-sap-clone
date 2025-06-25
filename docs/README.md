# 📚 SAP Clone Documentation

## 📋 **Documentation Index**

### **🚀 Project Setup & Reference**
- [**AWS-Free-Tier-Setup.md**](./AWS-Free-Tier-Setup.md) - 💰 Complete AWS Free Tier deployment guide ($0/month for 12 months!)
- [**AWS-CLI-Setup.md**](./AWS-CLI-Setup.md) - 🛠️ AWS CLI setup, commands, and automation scripts
- [**CI-CD-Pipeline.md**](./CI-CD-Pipeline.md) - 🚀 Modern CI/CD pipeline with GitHub Actions
- [**Free-Tier-vs-Enterprise.md**](./Free-Tier-vs-Enterprise.md) - 🆚 What we "missed" vs enterprise and migration paths

### **🏗️ Project Structure**
```
SAP-Clone/
├── 📁 docs/                    # 📚 All project documentation
├── 📁 .github/workflows/       # 🚀 CI/CD pipeline (AWS Free Tier)
├── 📁 src/                     # 💻 Source code
│   ├── 📁 Core/               # 🧠 Domain & Business Logic
│   ├── 📁 Infrastructure/     # 🔌 Data & External Services  
│   ├── 📁 API/               # 🚀 Web API (.NET 9)
│   └── 📁 Web/               # 🎨 Frontend (React 19 + Bun)
├── 📁 tests/                  # 🧪 Test projects
├── 🐳 docker-compose.yml      # Local database setup
├── 🐳 Dockerfile.api          # API containerization
├── 🐳 Dockerfile.frontend     # Frontend containerization
└── 🚀 run-dev.ps1            # Local development script
```

### **⚡ Quick Commands**
```powershell
# Start both backend and frontend
.\run-dev.ps1

# Start only backend
.\run-dev.ps1 -BackendOnly

# Start only frontend  
.\run-dev.ps1 -FrontendOnly
``` 