# Key Vault to AppSettings Mapping

This document maps configuration keys in `appsettings.json` to their corresponding Azure Key Vault secret names.

## Overview

PoCoupleQuiz uses Azure Key Vault for secure secret management in production. The application automatically loads secrets from Key Vault on startup using the `Azure.Extensions.AspNetCore.Configuration.Secrets` package.

## Key Vault Configuration

| Environment Variable | Description |
|---------------------|-------------|
| `KEYVAULT_URI` | Key Vault URI (e.g., `https://kv-poshared.vault.azure.net/`) |
| `SKIP_KEYVAULT` | Set to `true` to skip Key Vault loading (local dev/tests) |

## Secret Mapping Table

| AppSettings Key | Key Vault Secret Name | Description | Required |
|-----------------|----------------------|-------------|----------|
| `AzureOpenAI:Endpoint` | `AzureOpenAI--Endpoint` | Azure OpenAI service endpoint URL | ✅ Yes |
| `AzureOpenAI:Key` | `AzureOpenAI--Key` | Azure OpenAI API key | ✅ Yes |
| `AzureOpenAI:DeploymentName` | `AzureOpenAI--DeploymentName` | GPT model deployment name (e.g., `gpt-4o`) | ✅ Yes |
| `AzureStorage:ConnectionString` | `AzureStorage--ConnectionString` | Azure Table Storage connection string | ✅ Yes |
| `ApplicationInsights:ConnectionString` | `ApplicationInsights--ConnectionString` | App Insights instrumentation connection | ⚠️ Optional |

## Key Vault Secret Naming Convention

Azure Key Vault uses `--` as a delimiter for nested configuration keys (since `:` is not allowed in secret names).

**Example:**
- AppSettings: `AzureOpenAI:Endpoint`
- Key Vault: `AzureOpenAI--Endpoint`

## Local Development

For local development, secrets can be configured in:

1. **User Secrets** (recommended):
   ```powershell
   dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-openai.openai.azure.com/"
   dotnet user-secrets set "AzureOpenAI:Key" "your-api-key"
   dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-4o"
   ```

2. **Environment Variables**:
   ```powershell
   $env:AzureOpenAI__Endpoint = "https://your-openai.openai.azure.com/"
   $env:AzureOpenAI__Key = "your-api-key"
   ```

3. **Azurite** (for Table Storage):
   ```powershell
   # Automatically used when running via Aspire AppHost
   docker run -d --name azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
   ```

## Production Configuration

In Azure Container Apps, the Key Vault URI is set via Bicep/Aspire:

```bicep
// infra/resources.bicep
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: 'kv-poshared'
}

// Container App receives KEYVAULT_URI environment variable
```

## Fallback Behavior

| Scenario | Behavior |
|----------|----------|
| Key Vault available | Secrets loaded from Key Vault |
| Key Vault unavailable + secrets in config | Uses appsettings/env vars |
| OpenAI secrets missing | Falls back to `MockQuestionService` |
| Storage secrets missing | Uses Azurite (local) or fails (production) |

## Security Best Practices

1. ✅ **Never commit secrets** to source control
2. ✅ **Use Managed Identity** in production (no API keys needed)
3. ✅ **Rotate secrets** regularly in Key Vault
4. ✅ **Set SKIP_KEYVAULT=true** in test environments
5. ✅ **Use RBAC** for Key Vault access (not access policies)

## Troubleshooting

### "Key Vault access denied"
Ensure the app's managed identity has `Key Vault Secrets User` role on the vault.

### "Configuration key not found"
Check that the secret name uses `--` delimiter, not `:` or `.`.

### "MockQuestionService being used"
OpenAI secrets are missing - check Key Vault or local configuration.
