param (
    [string]$wpUrl,
    [string]$wpUserName,
    [string]$wpPassword,
    [string]$vstsMetadataFile,
    [string]$embedExternalImages,
    [string]$vstsToken,
    [string]$wpPublishAsCommitter,
    [string]$vstsProcessSubfolders,
    [string]$wpUseFolderNameAsCategory,
    [string]$wpUseFolderNameAsTag,
    [string]$wpPublishAsDraft,
    [string]$wpTrackPostIdInFilename,
    [string]$addEditLink,
    [string]$editLinkText,
    [string]$editLinkStyle
)

$vstsProjectName = $env:SYSTEM_TEAMPROJECT
$vstsRepoName = $env:BUILD_REPOSITORY_NAME
$vstsBranchName = $env:BUILD_SOURCEBRANCHNAME
$vstsAccount = $env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI

Write-Verbose "Entering: md2wp.ps1"
Write-Verbose "  Debug: $env:SYSTEM_DEBUG"

Write-Verbose "  wpUrl: $wpUrl"
Write-Verbose "  wpUserName: $wpUserName"
Write-Verbose "  wpPassword: ********"
Write-Verbose "  wpPublishAsDraft: $wpPublishAsDraft"
Write-Verbose "  wpPublishAsCommitter: $wpPublishAsCommitter"
Write-Verbose "  wpUseFolderNameAsCategory: $wpUseFolderNameAsCategory"
Write-Verbose "  wpUseFolderNameAsTag: $wpUseFolderNameAsTag"
Write-Verbose "  wpTrackPostIdInFilename: $wpTrackPostIdInFilename"

Write-Verbose "  vstsToken: ********"
Write-Verbose "  vstsMetadataFile BEFORE: $vstsMetadataFile"
$vstsMetadataFile = $vstsMetadataFile.Replace($env:BUILD_SOURCESDIRECTORY, "") # Convert to relative path (relative to source root)
$vstsMetadataFile = $vstsMetadataFile.Replace("C:\", "") # Remove leading "C:" if it exists (TODO: Find a better way to convert to relative path)
$vstsMetadataFile = $vstsMetadataFile.Replace("D:\", "") # Remove leading "D:" if it exists (TODO: Find a better way to convert to relative path)
Write-Verbose "  vstsMetadataFile AFTER: $vstsMetadataFile"
Write-Verbose "  vstsProcessSubfolders: $vstsProcessSubfolders"

Write-Verbose "  addEditLink: $addEditLink"
Write-Verbose "  editLinkText: $editLinkText"
Write-Verbose "  editLinkStyle: $editLinkStyle"
Write-Verbose "  embedExternalImages: $embedExternalImages"

Write-Verbose "  vstsAccount: $vstsAccount"
Write-Verbose "  vstsProjectName: $vstsProjectName"
Write-Verbose "  vstsRepoName: $vstsRepoName"
Write-Verbose "  vstsBranchName: $vstsBranchName"

# Import the Task.Common dll that has all the cmdlets we need for Build
import-module "Microsoft.TeamFoundation.DistributedTask.Task.Common"

$exe = "MD2WP.CLI.exe"

$currentDir = (Get-Item -Path ".\CLI" -Verbose).FullName

$exePath = [System.IO.Path]::Combine($currentDir, "$exe")

#$exeArgs = "`"$env:SYSTEM_DEBUG`" `"$wpUrl`" `"$wpUserName`" `"$wpPassword`" `"$vstsAccount`" `"$vstsProjectName`" `"$vstsRepoName`" `"$vstsBranchName`" `"$vstsToken`" `"$vstsMetadataFIle`" `"$embedExternalImages`" `"$wpPublishAsCommitter`" `"$vstsProcessSubfolders`" `"$wpUseFolderNameAsCategory`" `"$wpUseFolderNameAsTag`" `"$wpPublishAsDraft`" `"$wpTrackPostIdInFilename`" `"$addEditLink`" `"$editLinkText`" `"$editLinkStyle`""

Write-Verbose "EXE and ARGS: $exePath $exeArgs"

#& $exePath $exeArgs 2>&1
& $exePath `"$env:SYSTEM_DEBUG`" `"$wpUrl`" `"$wpUserName`" `"$wpPassword`" `"$vstsAccount`" `"$vstsProjectName`" `"$vstsRepoName`" `"$vstsBranchName`" `"$vstsToken`" `"$vstsMetadataFIle`" `"$embedExternalImages`" `"$wpPublishAsCommitter`" `"$vstsProcessSubfolders`" `"$wpUseFolderNameAsCategory`" `"$wpUseFolderNameAsTag`" `"$wpPublishAsDraft`" `"$wpTrackPostIdInFilename`" `"$addEditLink`" `"$editLinkText`" `"$editLinkStyle`" 2>&1
