# ADR 003: Use Azure Key Vault References for Application Secrets

## Status
Accepted

## Context
The application requires secure storage and access to sensitive configuration:
- Azure Table Storage connection strings
- Azure OpenAI API keys
- Application Insights instrumentation keys
- Other third-party API credentials

We evaluated several secret management approaches:
- **Configuration Files**: Easy but insecure (secrets in source control)
- **Environment Variables**: Better but difficult to rotate, no audit trail
- **Azure Key Vault**: Centralized, secure, auditable secret management
- **Managed Identity + Key Vault References**: Passwordless authentication with App Service

## Decision
We will use **Azure Key Vault with Key Vault References** in Azure App Service, backed by System-Assigned Managed Identity.

## Rationale
1. **Security**: Secrets never stored in configuration files or environment variables
2. **Passwordless**: Managed Identity eliminates the need for service principal credentials
3. **Auditability**: Key Vault logs all secret access attempts
4. **Rotation**: Secrets can be rotated in Key Vault without redeploying the app
5. **Separation of Concerns**: Infrastructure team manages Key Vault, developers reference secrets
6. **Azure Integration**: Native support in App Service with `@Microsoft.KeyVault(...)` syntax

## Consequences

### Positive
- No secrets in source control or configuration files
- Automatic secret rotation without application redeployment
- Full audit trail of secret access in Key Vault logs
- Reduced attack surface (no credentials to steal)
- Centralized secret management across multiple environments
- App Service automatically fetches secrets at startup

### Negative
- Requires Bicep/ARM template configuration for Managed Identity
- Local development still uses User Secrets (different pattern)
- Initial setup complexity (Key Vault, Managed Identity, RBAC)
- Debugging secret access issues requires Azure Portal access
- App Service restart required if secrets are updated (to refresh)

## Implementation Notes

### Bicep Configuration
```bicep
// Enable System-Assigned Managed Identity on App Service
resource appService 'Microsoft.Web/sites@2023-01-01' = {
  identity: {
    type: 'SystemAssigned'
  }
}

// Grant App Service Get/List permissions on Key Vault
resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: appService.identity.principalId
        permissions: {
          secrets: ['get', 'list']
        }
      }
    ]
  }
}
```

### App Service Configuration
Use Key Vault References in App Service configuration:
```
AzureTableStorage__ConnectionString = @Microsoft.KeyVault(SecretUri=https://myvault.vault.azure.net/secrets/TableStorageConnectionString/)
AzureOpenAI__ApiKey = @Microsoft.KeyVault(SecretUri=https://myvault.vault.azure.net/secrets/OpenAIApiKey/)
```

### Local Development
For local development, use .NET User Secrets:
```powershell
dotnet user-secrets set "AzureTableStorage:ConnectionString" "UseDevelopmentStorage=true"
dotnet user-secrets set "AzureOpenAI:ApiKey" "local-development-key"
```

### Health Checks
Implement health checks that verify:
- Managed Identity is properly configured
- Key Vault is accessible
- Required secrets exist and are valid

## Security Considerations
- Use **Least Privilege**: Grant only `get` and `list` permissions, not `set` or `delete`
- Enable **Soft Delete** on Key Vault to prevent accidental secret deletion
- Use **Diagnostic Settings** to send Key Vault logs to Log Analytics
- Rotate secrets regularly using Key Vault versioning
- Separate Key Vaults per environment (dev, staging, production)

## Alternatives Considered

### Environment Variables
- **Pros**: Simple, widely supported
- **Cons**: Secrets visible in Azure Portal, no audit trail, hard to rotate
- **Why not chosen**: Less secure, no secret rotation

### Azure App Configuration
- **Pros**: Centralized configuration, feature flags
- **Cons**: Additional service cost, more complex than needed
- **Why not chosen**: Key Vault is sufficient for secrets-only scenarios

## References
- [Key Vault References in App Service](https://learn.microsoft.com/en-us/azure/app-service/app-service-key-vault-references)
- [Managed Identity Documentation](https://learn.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)
- [Key Vault Best Practices](https://learn.microsoft.com/en-us/azure/key-vault/general/best-practices)
