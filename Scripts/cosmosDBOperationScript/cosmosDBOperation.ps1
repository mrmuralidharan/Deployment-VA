
#Install-Module -Name Az -Scope CurrentUser -Repository PSGallery -Force
#Install-Module -Name CosmosDB
Import-Module -Name CosmosDB
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
#Connecting with the Azure account
Connect-AzAccount -TenantId 73e2dc65-e18d-4421-8da7-e96d82b63aae -SubscriptionId 98590240-1726-40d9-a6ea-8fdc1fc448b3
#Connect-AzAccount

#Please provide the path to the json files
Write-Host "Getting the json to run the script"
$cosmosDetails=Get-Content -Path "C:\VirtualAssistant\deployment_Error\Scripts\cosmosDBOperationScript\cosmosDB-Creation-Details.json" | ConvertFrom-Json

<#
$helpDocumentValues=Get-Content -Path "C:\VirtualAssistant\HelpCosmosDB.json" | ConvertFrom-Json
$acronymDocumentValues=Get-Content -Path "C:\VirtualAssistant\AcronymCosmosDB.json" | ConvertFrom-Json
$departmentDocumentValues=Get-Content -Path  "C:\VirtualAssistant\departmentLookupCosmosDB_New.json" | ConvertFrom-Json
#>

foreach ($json in $cosmosDetails){
    $resourceGroupName = $json.resourceGroupName # Resource Group must already exist
    $accountName = $json.cosmosDbAccountName # Must be all lower case
    $consistencyLevel = "Session"
    $databaseName = $json.cosmosDbName
    $containerName = $json.cosmosContainerName
    $autoscaleMaxThroughput = 4000 #minimum = 4000
    $partitionKeyPath = $json.partitionKeyPath

    $cosmosDbContext = New-CosmosDbContext -Account $accountName -Database $databaseName -ResourceGroup $resourceGroupName

    Write-Host "Creating database $databaseName"
    $database = New-AzCosmosDBSqlDatabase -AccountName $accountName -Name $databaseName -ResourceGroupName $resourceGroupName

    Write-Host "Creating container $containerName"
    $container = New-AzCosmosDBSqlContainer -AccountName $accountName -DatabaseName $databaseName -ResourceGroupName $resourceGroupName -Name $containerName -PartitionKeyKind Hash -PartitionKeyPath $partitionKeyPath -AutoscaleMaxThroughput $autoscaleMaxThroughput

    Write-Host "Getting the file content from "$($json.containerItemsValue)""
    $skillContainerItems=Get-Content -Path $json.containerItemsValue | ConvertFrom-Json

    Write-Host "Creating the items in the container $containerName"
    if($json.skillName -eq "Department")
    {
        foreach ($json in $skillContainerItems)
        {
            $document = @"
            {
    
                "id": "$([Guid]::NewGuid().ToString())",
                "deptName": "$($json.deptName)",
                "deptNumber": "$($json.deptNumber)",
                "type": "$($json.type)"
            }
"@
            New-CosmosDbDocument -Context $cosmosDbContext -CollectionId $containerName -DocumentBody $document -PartitionKey $json.deptNumber.ToString()
        }
    }
    <#
    elseif($json.skillName -eq "Help"){
        foreach ($json in $skillContainerItems)
        {
            $document = @"
            {
    
                "id": "$([Guid]::NewGuid().ToString())",
                "type": "$($json.type)",
                "O365helpCommands": "$($json.O365helpCommands)",
                "PagerhelpCommands": "$($json.PagerhelpCommands)",
                "ServicenowhelpCommands": "$($json.ServicenowhelpCommands)",
                "DepartmenthelpCommands": "$($json.DepartmenthelpCommands)",
                "WeatherhelpCommands": "$($json.WeatherhelpCommands)",
            }
"@
            New-CosmosDbDocument -Context $cosmosDbContext -CollectionId $containerName -DocumentBody $document -PartitionKey $json.type.ToString()
        }
    }
    #>
     elseif($json.skillName -eq "Acronym"){
        foreach ($json in $skillContainerItems)
        {
            $document = @"
            {
    
                "id": "$([Guid]::NewGuid().ToString())",
                "Acronym": "$($json.Acronym)",
                "Description": "$($json.Description)"
            }
"@
            New-CosmosDbDocument -Context $cosmosDbContext -CollectionId $containerName -DocumentBody $document -PartitionKey $json.Acronym.ToString()
        }
    }

}

Write-Host "Script Completed!!!"