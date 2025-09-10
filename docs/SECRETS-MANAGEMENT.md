# 🔐 BTHL CheckGate - Secrets Management Guide

## Security-First Configuration

**We prioritize** secure credential management as a fundamental security practice. **This guide** provides comprehensive instructions for properly managing secrets, passwords, and configuration in production environments.

---

## 🚨 Critical Security Requirements

### Never Commit Secrets
- **❌ NEVER** commit passwords, API keys, or tokens to source control
- **❌ NEVER** store secrets in configuration files within the repository  
- **❌ NEVER** use hardcoded credentials in production environments
- **✅ ALWAYS** use environment variables or dedicated secret management systems

### Production Deployment Checklist
- [ ] All placeholder credentials (`CHANGEME123`) have been replaced
- [ ] Database passwords are generated and securely stored
- [ ] JWT secret keys are cryptographically secure (256-bit minimum)
- [ ] Environment variables are configured in deployment environment
- [ ] Secret management system is properly configured
- [ ] Access to secrets is logged and monitored

---

## 🔑 Secret Generation

### Database Credentials
```bash
# Generate secure database password (32 characters, mixed case, numbers, symbols)
openssl rand -base64 32

# Alternative using PowerShell
Add-Type -AssemblyName System.Web
[System.Web.Security.Membership]::GeneratePassword(32, 8)
```

### JWT Secret Keys
```bash
# Generate 256-bit JWT secret (Linux/macOS)
openssl rand -hex 32

# Generate JWT secret using PowerShell (Windows)
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RNGCryptoServiceProvider]::new().GetBytes($bytes)
[Convert]::ToBase64String($bytes)

# Verify key length (should be 256 bits / 32 bytes minimum)
echo "your_key_here" | base64 -d | wc -c
```

### Encryption Keys
```bash
# Generate AES-256 encryption key
openssl rand -hex 32

# Generate using .NET
[System.Security.Cryptography.Aes]::Create().GenerateKey() | ForEach-Object { [Convert]::ToHex($_) }
```

---

## 🏗️ Environment-Specific Configuration

### Development Environment
```bash
# .env file (never commit to repository)
DB_HOST=localhost
DB_PORT=3306
DB_NAME=bthl_checkgate_dev
DB_USER=dev_user
DB_PASSWORD=your_generated_dev_password
JWT_SECRET=your_generated_dev_jwt_secret
ASPNETCORE_ENVIRONMENT=Development
```

### Production Environment
```bash
# Production environment variables (set via deployment system)
export DB_CONNECTION_STRING="Server=prod-server;Database=bthl_checkgate;Uid=bthl_user;Pwd=your_secure_password;AllowUserVariables=True"
export JWT_SECRET="your_256_bit_production_jwt_secret"
export ENCRYPTION_KEY="your_256_bit_encryption_key"
export ASPNETCORE_ENVIRONMENT=Production
```

---

## ☁️ Secret Management Systems

### Azure Key Vault
```bash
# Create Key Vault
az keyvault create --name bthl-checkgate-kv --resource-group bthl-rg --location eastus

# Store database password
az keyvault secret set --vault-name bthl-checkgate-kv --name db-password --value "your_secure_password"

# Store JWT secret
az keyvault secret set --vault-name bthl-checkgate-kv --name jwt-secret --value "your_256_bit_jwt_secret"

# Retrieve secrets in application
# Configure Azure.Security.KeyVault.Secrets NuGet package
```

### AWS Secrets Manager
```bash
# Create secret
aws secretsmanager create-secret --name bthl-checkgate/db-credentials \
    --description "Database credentials for BTHL CheckGate" \
    --secret-string '{"username":"bthl_user","password":"your_secure_password"}'

# Create JWT secret
aws secretsmanager create-secret --name bthl-checkgate/jwt-secret \
    --description "JWT signing secret" \
    --secret-string "your_256_bit_jwt_secret"
```

### HashiCorp Vault
```bash
# Enable KV secrets engine
vault secrets enable -path=bthl-checkgate kv-v2

# Store database credentials
vault kv put bthl-checkgate/database \
    username=bthl_user \
    password=your_secure_password

# Store JWT secret
vault kv put bthl-checkgate/jwt \
    secret=your_256_bit_jwt_secret
```

---

## 🐳 Container Deployment

### Docker Compose
```yaml
version: '3.8'
services:
  bthl-checkgate:
    image: bthl-checkgate:latest
    environment:
      - DB_CONNECTION_STRING
      - JWT_SECRET
      - ENCRYPTION_KEY
    secrets:
      - db_password
      - jwt_secret
    
secrets:
  db_password:
    external: true
  jwt_secret:
    external: true
```

### Kubernetes Secrets
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: bthl-checkgate-secrets
  namespace: bthl-checkgate
type: Opaque
data:
  db-password: <base64-encoded-password>
  jwt-secret: <base64-encoded-jwt-secret>
  encryption-key: <base64-encoded-encryption-key>
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: bthl-checkgate
spec:
  template:
    spec:
      containers:
      - name: bthl-checkgate
        image: bthl-checkgate:latest
        env:
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: bthl-checkgate-secrets
              key: db-password
        - name: JWT_SECRET
          valueFrom:
            secretKeyRef:
              name: bthl-checkgate-secrets
              key: jwt-secret
```

---

## 🔄 Secret Rotation

### Automated Rotation Strategy
```bash
#!/bin/bash
# rotate-secrets.sh - Example secret rotation script

# Generate new JWT secret
NEW_JWT_SECRET=$(openssl rand -hex 32)

# Update in secret management system
az keyvault secret set --vault-name bthl-checkgate-kv --name jwt-secret --value "$NEW_JWT_SECRET"

# Trigger application restart to pick up new secret
kubectl rollout restart deployment/bthl-checkgate

# Log rotation event
echo "$(date): JWT secret rotated" >> /var/log/bthl-checkgate/secret-rotation.log
```

### Rotation Schedule
- **Database passwords**: Every 90 days
- **JWT secrets**: Every 30 days  
- **Encryption keys**: Every 180 days
- **API keys**: Every 60 days

---

## 🛡️ Security Best Practices

### Access Control
- **Principle of least privilege**: Grant minimum required access
- **Role-based access control**: Use RBAC for secret access
- **Audit logging**: Log all secret access and modifications
- **Multi-factor authentication**: Require MFA for secret management access

### Secret Handling in Code
```csharp
// ✅ GOOD: Retrieve secrets from configuration/environment
public class DatabaseService
{
    private readonly string _connectionString;
    
    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Database connection string not configured");
    }
}

// ❌ BAD: Hardcoded secrets
public class DatabaseService
{
    private const string CONNECTION_STRING = "Server=prod;Uid=root;Pwd=hardcoded123;"; // Never do this!
}
```

### Monitoring and Alerting
```yaml
# Example alert configuration
alerts:
  - name: secret-access-anomaly
    condition: secret_access_rate > normal_threshold
    action: notify_security_team
    
  - name: secret-rotation-overdue
    condition: days_since_rotation > rotation_policy_days
    action: notify_ops_team
```

---

## 📋 Compliance and Auditing

### Security Standards Compliance
- **SOC 2**: Implement proper access controls and audit logging
- **PCI DSS**: Use secure key management for payment card environments
- **GDPR**: Ensure data encryption keys are properly managed
- **HIPAA**: Implement secure key management for healthcare environments

### Audit Requirements
- **Access logs**: Who accessed which secrets when
- **Modification logs**: Who changed secrets when
- **Rotation logs**: When secrets were rotated
- **Failure logs**: Failed access attempts and security events

---

## 🚨 Incident Response

### Compromised Secrets Response Plan
1. **Immediate actions** (within 5 minutes):
   - Revoke compromised secrets
   - Generate new secrets
   - Update applications with new secrets
   
2. **Short-term actions** (within 1 hour):
   - Audit access logs for unauthorized usage
   - Identify affected systems and data
   - Notify security team and stakeholders
   
3. **Long-term actions** (within 24 hours):
   - Conduct full security assessment
   - Update security policies if needed
   - Implement additional monitoring

### Emergency Secret Rotation
```bash
#!/bin/bash
# emergency-rotation.sh - Emergency secret rotation script

echo "EMERGENCY: Rotating all secrets immediately"

# Rotate database password
NEW_DB_PASSWORD=$(openssl rand -base64 32)
az keyvault secret set --vault-name bthl-checkgate-kv --name db-password --value "$NEW_DB_PASSWORD"

# Rotate JWT secret
NEW_JWT_SECRET=$(openssl rand -hex 32)
az keyvault secret set --vault-name bthl-checkgate-kv --name jwt-secret --value "$NEW_JWT_SECRET"

# Force application restart
kubectl rollout restart deployment/bthl-checkgate

echo "EMERGENCY: All secrets rotated, applications restarting"
```

---

**We implement** these secrets management practices to ensure BTHL CheckGate meets enterprise security standards and protects sensitive information throughout its lifecycle. **Our approach** demonstrates security-first design principles that are essential for production deployments.

*For additional security guidance, consult with your organization's security team and follow your internal security policies and procedures.*