# GitHub Actions Setup Instructions

## Setting up the Publish Profile Secret

To enable automatic deployment via GitHub Actions, you need to add the Azure App Service publish profile as a repository secret.

### Steps:

1. **Copy the Publish Profile**:
   Copy this XML content (the publish profile for PoCoupleQuiz):
   ```xml
   <publishData><publishProfile profileName="PoCoupleQuiz - Web Deploy" publishMethod="MSDeploy" publishUrl="pocouplequiz.scm.azurewebsites.net:443" msdeploySite="PoCoupleQuiz" userName="$PoCoupleQuiz" userPWD="JtpFfkgzNeJtxJpoYJawPQ8mEHKmgoL7lJSAAawAaBTPugBcaLhmGxpqJEr2" destinationAppUrl="https://pocouplequiz.azurewebsites.net" SQLServerDBConnectionString="" mySQLDBConnectionString="" hostingProviderForumLink="" controlPanelLink="https://portal.azure.com" webSystem="WebSites"><databases /></publishProfile><publishProfile profileName="PoCoupleQuiz - FTP" publishMethod="FTP" publishUrl="ftps://waws-prod-bn1-261.ftp.azurewebsites.windows.net/site/wwwroot" ftpPassiveMode="True" userName="PoCoupleQuiz\$PoCoupleQuiz" userPWD="JtpFfkgzNeJtxJpoYJawPQ8mEHKmgoL7lJSAAawAaBTPugBcaLhmGxpqJEr2" destinationAppUrl="https://pocouplequiz.azurewebsites.net" SQLServerDBConnectionString="" mySQLDBConnectionString="" hostingProviderForumLink="" controlPanelLink="https://portal.azure.com" webSystem="WebSites"><databases /></publishProfile><publishProfile profileName="PoCoupleQuiz - Zip Deploy" publishMethod="ZipDeploy" publishUrl="pocouplequiz.scm.azurewebsites.net:443" userName="$PoCoupleQuiz" userPWD="JtpFfkgzNeJtxJpoYJawPQ8mEHKmgoL7lJSAAawAaBTPugBcaLhmGxpqJEr2" destinationAppUrl="https://pocouplequiz.azurewebsites.net" SQLServerDBConnectionString="" mySQLDBConnectionString="" hostingProviderForumLink="" controlPanelLink="https://portal.azure.com" webSystem="WebSites"><databases /></publishProfile></publishData>
   ```

2. **Add GitHub Repository Secret**:
   - Go to your GitHub repository
   - Click **Settings** → **Secrets and variables** → **Actions**
   - Click **New repository secret**
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: Paste the XML content from step 1
   - Click **Add secret**

3. **Test the Pipeline**:
   - Make any change to your code
   - Commit and push to the `main` or `master` branch
   - Check the **Actions** tab to see the workflow running
   - The deployment will automatically update https://pocouplequiz.azurewebsites.net

## Workflow Features

The GitHub Actions workflow (`.github/workflows/main.yml`) includes:
- ✅ Automated build with .NET 9.0
- ✅ Unit test execution
- ✅ Application publishing
- ✅ Deployment to Azure App Service
- ✅ Post-deployment health check validation

Once the secret is configured, every push to main/master will automatically deploy your application!
