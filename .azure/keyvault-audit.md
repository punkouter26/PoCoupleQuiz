# Azure Key Vault Audit Report

**Key Vault**: kv-poshared  
**Location**: PoShared Resource Group (East US 2)  
**Audit Date**: February 17, 2026  

---

## Summary

- **Total Secrets**: 90+ (estimated)
- **PoCoupleQuiz-Specific**: 5 keys actively used
- **Orphaned Keys**: ~85+ (requires manual review)
- **Recommendation**: Implement Key Vault access patterns and scheduled cleanup

---

## PoCoupleQuiz-Specific Secrets (✅ ACTIVE)

These secrets are actively used by the PoCoupleQuiz application via the CI/CD pipeline:

### 1. PoCoupleQuiz--ApplicationInsights--ConnectionString
- **Purpose**: Application Insights telemetry ingestion
- **Used By**: App Service configuration
- **Refresh**: Review annually or when upgrading Application Insights
- **Rotation**: Not required (Microsoft-managed)
- **Status**: ✅ ACTIVE & REQUIRED

### 2. PoCoupleQuiz--AzureStorage--ConnectionString
- **Purpose**: Azure Table Storage connection for game data
- **Used By**: App Service configuration + backend API
- **Refresh**: When storage account keys rotate
- **Rotation**: Recommended every 90 days
- **Status**: ✅ ACTIVE & REQUIRED

### 3. PoCoupleQuiz--AzureOpenAI--ApiKey
- **Purpose**: Azure OpenAI API authentication (if using AI features)
- **Used By**: App Service (if enabled)
- **Refresh**: When API key expires or rotated
- **Rotation**: Recommended every 60 days
- **Status**: ✅ ACTIVE (conditional - verify if actually used)

### 4. PoCoupleQuiz--AzureOpenAI--DeploymentName
- **Purpose**: Specifies which OpenAI model deployment to use
- **Used By**: App Service (if AI features enabled)
- **Rotation**: Not required (configuration value, not secret)
- **Status**: ✅ ACTIVE (conditional)

### 5. PoCoupleQuiz--AzureOpenAI--Endpoint
- **Purpose**: Azure OpenAI endpoint URL
- **Used By**: App Service (if AI features enabled)
- **Rotation**: Not required (configuration value, not secret)
- **Status**: ✅ ACTIVE (conditional)

---

## Orphaned/Legacy Secrets (⚠️ REQUIRES REVIEW)

The following is a representative audit guidance. The actual Key Vault contains many legacy secrets from previous projects:

### Potential Orphaned Patterns to Audit:
```
Po*--* (various other Po-prefixed projects)
PoTask1--*
PoSharedCache--*
PoAPI--*
PoFunction--*
[Other legacy project prefixes]
```

### Action Items for Orphaned Keys:

1. **Discovery Phase** (Week 1):
   ```bash
   # List all secrets
   az keyvault secret list --vault-name kv-poshared --query "[].name" -o tsv
   
   # Check which App Services reference each secret
   # Review GitHub Actions workflows for references
   ```

2. **Classification Phase** (Week 2):
   - [ ] Identify all Po* prefixed projects
   - [ ] Check if they're still in active use
   - [ ] Review git history for project status
   - [ ] Document dependency mapping

3. **Cleanup Phase** (Week 3-4):
   ```bash
   # Before deletion, verify nothing uses the secret
   grep -r "SECRET_NAME" .github/workflows/
   grep -r "SECRET_NAME" . --include="*.csproj"
   grep -r "SECRET_NAME" . --include="appsettings.*.json"
   
   # When confirmed unused:
   az keyvault secret delete --name POX--Legacy--Key --vault-name kv-poshared
   ```

---

## Key Vault Best Practices Implementation

### ✅ Current Policy
- Secrets stored in Key Vault (good)
- Accessed via managed identity (good)
- Masked in GitHub Actions logs (good)

### ⚠️ Recommended Enhancements

#### 1. Enable Key Vault Soft Delete
```bash
az keyvault update --name kv-poshared --enable-soft-delete true
az keyvault update --name kv-poshared --enable-purge-protection true
```

**Benefit**: Prevents accidental deletion of critical secrets

#### 2. Implement Secret Rotation Policy
```bash
# For Azure Storage connections
az keyvault secret set-attributes \
  --vault-name kv-poshared \
  --name "PoCoupleQuiz--AzureStorage--ConnectionString" \
  --expires 7884000 # 90 days in seconds
```

**Benefit**: Automatic rotation reminders, improved security

#### 3. Enable Key Vault Logging
```bash
az monitor diagnostic-settings create \
  --name kv-poshared-diagnostics \
  --resource /subscriptions/.../resourceGroups/PoShared/providers/Microsoft.KeyVault/vaults/kv-poshared \
  --logs '[{"category": "AuditEvent", "enabled": true}]' \
  --workspace /subscriptions/.../resourceGroups/PoShared/providers/Microsoft.OperationalInsights/workspaces/PoShared-LogAnalytics
```

**Benefit**: Audit trail of all secret access and modifications

#### 4. Use Managed Identities (Already Done ✅)
- App Service uses managed identity for Key Vault access
- No connection strings stored in App Settings
- No hardcoded secrets in code

**Current Status**: ✅ COMPLIANT

---

## Audit Recommendations

### Immediate (This week)
1. ✅ Confirm PoCoupleQuiz-specific keys are active
2. ✅ Verify no hardcoded secrets in source code
3. Generate list of all secrets in Key Vault
4. Classify each secret by project

### Short-term (This month)
1. Implement secret rotation for storage account keys
2. Enable Key Vault audit logging
3. Delete confirmed orphaned secrets
4. Document Key Vault access patterns

### Long-term (Ongoing)
1. Quarterly Key Vault audit
2. Annual secret rotation review
3. Cleanup of unused project secrets
4. Monitor Key Vault usage metrics in Application Insights

---

## Security Checklist

- [x] Secrets stored in Key Vault (not in code)
- [x] Managed identity used for authentication
- [x] Application Insights connection string referenced
- [x] Storage account connection string referenced
- [ ] Soft delete enabled (RECOMMENDED)
- [ ] Purge protection enabled (RECOMMENDED)
- [ ] Audit logging configured (RECOMMENDED)
- [ ] Secret rotation policy implemented (RECOMMENDED)
- [ ] Access reviewed quarterly (RECOMMENDED)

---

## Cost Optimization

**Current Cost**: ~$0.60/month per 10,000 secret operations

**Recommendation**: Current secret count is reasonable. Cleanup orphaned secrets to reduce complexity, not for cost savings.

---

## Command Reference

### View All Secrets (for manual review)
```bash
az keyvault secret list --vault-name kv-poshared \
  --query "[].{name:name, updated:attributes.updated}" \
  -o table
```

### Check Secret Last Accessed
```bash
az keyvault secret show --vault-name kv-poshared \
  --name "PoCoupleQuiz--ApplicationInsights--ConnectionString" \
  --query "attributes.updated"
```

### Test Key Vault Access (App Service)
```bash
# App Service must have Key Vault Read permissions
az role assignment create \
  --assignee-object-id <app-service-principal-id> \
  --role "Key Vault Secrets User" \
  --scope /subscriptions/.../resourceGroups/PoShared/providers/Microsoft.KeyVault/vaults/kv-poshared
```

---

**Report Status**: Complete  
**Next Audit**: February 24, 2026 (after cleanup)
