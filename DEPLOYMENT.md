# 🚀 BTHL CheckGate - Deployment Guide

## 📋 Prerequisites Verified

Your system has:
- ✅ **MySQL 8.0** installed at `C:\Program Files\MySQL\MySQL Server 8.0\bin`
- ✅ **MySQL Credentials**: localhost, root, password: CHANGEME123
- 🔄 **.NET 9** (will be verified during setup)
- 🔄 **Docker Desktop** (optional, for Kubernetes monitoring)

## 🎯 Quick Start (2 Steps)

### Step 1: Complete Deployment
Run as **Administrator** in PowerShell:
```powershell
cd C:\Users\Administrator\dev\BTHL-CheckGate
.\deployment\scripts\deploy-bthl-checkgate.ps1
```
*This single script handles database creation, dependency checking, and building*

### Step 2: Start Application
```powershell
dotnet run --project src/BTHLCheckGate.Service -- --console
```

## 🌐 Access Points

Once running:
- **Main Dashboard**: https://localhost:9300
- **API Documentation**: https://localhost:9300/api/docs
- **Health Check**: https://localhost:9300/health

## 🧪 Test the API

Use the included test client:
```powershell
.\client\test-client\BTHLCheckGate.TestClient.exe --interactive
```

## 📊 What You'll See

**Real-time Monitoring:**
- CPU, Memory, Disk, Network metrics
- System process monitoring
- Performance alerts and thresholds

**Kubernetes Integration:**
- Docker Desktop cluster status (if available)
- Pod and node monitoring
- Container resource tracking

**Enterprise Features:**
- JWT authentication
- Rate limiting
- Comprehensive logging
- API documentation

## 🔧 Alternative Manual Setup

If scripts fail, manually:

1. **Create Database:**
```sql
-- Connect to MySQL as root
CREATE DATABASE bthl_checkgate CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
SOURCE database/schema/01-initial-schema.sql;
```

2. **Build Solution:**
```powershell
dotnet build BTHLCheckGate.sln --configuration Release
```

3. **Run Service:**
```powershell
dotnet run --project src/BTHLCheckGate.Service -- --console --port 9300
```

## 🚨 Troubleshooting

**Database Connection Issues:**
- Ensure MySQL service is running
- Verify credentials: root / CHANGEME123
- Check Windows Firewall settings

**Build Issues:**
- Install .NET 8 SDK from Microsoft
- Run `dotnet restore` first

**Port 9300 In Use:**
- Stop existing services on port 9300
- Or change port in configuration

## 🎉 Success Indicators

✅ Database created with 7+ tables  
✅ Solution builds without errors  
✅ Service starts and listens on port 9300  
✅ Web dashboard loads at https://localhost:9300  
✅ API returns system metrics  

## 📈 Next Steps

1. **Production Deployment**: Use Windows Service installer
2. **Security**: Change default JWT secret key
3. **Monitoring**: Configure alert thresholds
4. **Scaling**: Add additional monitoring endpoints

Your BTHL CheckGate enterprise monitoring platform is ready to demonstrate comprehensive system and Kubernetes monitoring capabilities!