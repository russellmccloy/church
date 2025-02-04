# Step 1:
Connect-AzAccount -UseDeviceAuthentication

# Step 2:
# (Optional) If you have multiple subscriptions, select a default subscription to deploy the resources.
$context = Get-AzSubscription -SubscriptionName 'RussDefault'
Set-AzContext $context

$resourceGroupName = 'church-dev01-rg'
$location = 'australiaeast'

# Step 3:
# Create the resource group.
$expiry = ((Get-Date).AddDays(1)).ToString('dd/MM/yyyy')
New-AzResourceGroup -Name $resourceGroupName -Location $location -Force -Tag @{ Owner = 'Russ'; Purpose = 'To contain the resources for Church.'; Expiry = $expiry }

# (Optional) set the default resource group for the subsequent deployments
Set-AzDefault -ResourceGroupName $resourceGroupName

# Step 4:
# (Optional) Run Bicep Linter
az bicep build -f .\infra\main.bicep

# Generate a unique deployment name.
$deploymentName = 'church-deployment-' + (Get-Date).ToString('yyyyMMddThhmmss')


# Step 6:
# Deploy to Azure
New-AzResourceGroupDeployment `
    -Name $deploymentName `
    -ResourceGroupName $resourceGroupName `
    -TemplateFile '.\infra\main.bicep' `
    -TemplateParameterFile .\infra\environments\main.dev.bicepparam

# Step 7:
# Clean up resources
# Remove-AzResourceGroup -Name $resourceGroupName -Force

# Google Sheets
# az keyvault secret set --vault-name church-dev-kv-01 --name googleSheetCredentialsJson --value @church-ui/credentialsdev.json

# Azure secret
# az keyvault secret set --vault-name church-dev-kv-01 --name azureAdClientSecret --value 2cC8Q~SCZVv~U.cYnnOsBBb434qhv6_CwCFngc5R

# storageConnectionString
# az keyvault secret set --vault-name church-dev-kv-01 --name storageConnectionString --value 'DefaultEndpointsProtocol=https;AccountName=churchdevst01;AccountKey=N5OxEMHX9zSUe+O2oI8PqGfDyNaGenuKETjg4PdfDJ2h6Ty823o56NE6D9g1e0bcyNRXggLwO2UP+AStVB7acg==;EndpointSuffix=core.windows.net'
