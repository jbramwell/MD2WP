# Markdown to WordPress
## **PREVIEW RELEASE**
This extension provides a build task that can publish markdown files stored within a VSTS Git repo to a WordPress blog. If you've ever wanted to source your blog from a version control system, specifically VSTS, then this extension can help you out.

>NOTE: Currently, this extension works only with Git repos in Visual Studio Team Services (VSTS). As of right now it does not work with Team Foundation Server (TFS, on-premises). We hope to make a TFS-compatible version in the near future.

Getting started is as easy as 1, 2, 3:
1. Add some markdown files to a Git repo in VSTS.
2. Create a new build definition and add the **Markdown to WordPress** build task.
3. Configure the build task settings and queue a build. Once the build has completed, your markdown files will be published (as a Draft or "Live" - your choice) to your WordPress blog.

After your markdown files are published for the first time a Mapping File is created (see *Team Services Settings* below) that contains various metadata about each of the files that were published. There are a few reasons you might want to edit this file directly:
* To associate Categories and/or Tags with a particular post.
* To change the blog post title. By default it will take the name of the markdown file (minus the .md extension).
* If you rename a markdown file you will also need to rename the corresponding entry in the Mapping File. Otherwise, the Markdown to WordPress build task will think the renamed file is new and it will get published as a new post. **NOTE**: If you check the *Track ID in Filename* option you won't need to manually update this file.
* **Most importantly**, if *Publish New Post as Draft* is checked, you will need to manually change the value of "IsPublic" to *true* before the post is published for the world to see (i.e. non-draft).

## Build Task Parameters
### WordPress Settings
|  Name                      | Description                                                                                                        |
|----------------------------|--------------------------------------------------------------------------------------------------------------------|
| WordPress URL              | The URL for the destination WordPress site.                                                                        |
| Admin Account              | The user name used to sign in to WordPress. **NOTE**: This user must be in the Administrator role.                 |
| Admin Password             | The password used to sign in to WordPress. It is recommended that you use a secret variable to provide this value. **NOTE** Two factor authentication with WordPress is not supported at this time.|
| Publish New Posts as Draft | If checked, new posts will be published as a Draft and you will need to modify the corresponding entry in the Mapping File to make the post public; Otherwise, if unchecked, new posts will be made public immediately. |
| Publish as Commit Author   | If checked, the post will be published using the WordPress account that matches the e-mail address of the markdown file committer. The associated WordPress user must have 'author' rights. Otherwise, the posts will be published using the WordPress credentials provided to this build task. |
| Categorize by Folder Name  | If checked, the folder name containing the markdown file will also be associated as a Category with the post.         |
| Tag with Folder Name       | If checked, the folder name containing the markdown file will also be associated as a Tag with the post.              |
| Track ID in Filename       | If checked, the post ID will be added to the end of the markdown filename. This will allow the file to be properly tracked if it is moved and/or renamed. Checking this option is recommended. **NOTE**: With this option checked, a new file will be renamed during the next build. You will need to resync you Git repo to ensure you have the properly named file once the build is completed. |

### Team Services Settings
|  Name                     | Description                                                                                                          |
|---------------------------|----------------------------------------------------------------------------------------------------------------------|
| VSTS Access Token         | The Personal Access Token used to access the VSTS project containing the markdown files to be published. It is recommended that you use a secret variable to provide this value. |
| Mapping File              | The name of the mapping file (e.g. _md2wp.json) used to track information related to each published markdown file. If the file does not exist, it will be created the first time the task is run. |
| Process Subfolders        | If true, all subfolders will be processed; Otherwise, only the folder containing the Mapping File will be processed. |

### General Settings
|  Name                     | Description                                                                                                          |
|---------------------------|----------------------------------------------------------------------------------------------------------------------|
| Add Edit Link             | If checked, an 'Edit this Page' link will be added to the bottom of the published document. This provides an easy mechanism for editing the published document at the source (e.g. in VSTS/TFS). |
| Edit Link Text            | If 'Add Edit Link' is checked, this is the text that will be displayed as a link to the source (markdown) file in VSTS/TFS. |
| Embed External Images     | If true, external images (images source from outside VSTS/TFS) will be embedded as Base64-encoded data; Otherwise, external images will simply be linked to. |

## Release History/Road Map
|Release |Description                                              |
|--------|---------------------------------------------------------|
| 0.2.8  | Added 'Add Edit Link' option.                           | 
| 0.2.1  | Added \*\*\*NO_CI\*\*\* to commit comments.             | 
| 0.2.0  | Several updates/fixes:                                  | 
|        | - Added ability to track ID in filename                 |
|        | - File renames now detected and published               |
|        | - Fixed bug where Published post reverted back to Draft |
| 0.1.60 | First preview release                                   |

## Notes, Known Issues and Other Information
* Does not work with two factor authentication at this time. If you have two factor authentication enabled on your WordPress site this extension will not work.
* If you have the *Track ID in Filename* option unchecked, and you rename a folder containing a published markdown file you will need to modify the respective path stored in the Mapping File. Otherwise the build task will see this as a new file and will publish the markdown file again.
* The markdown files are converted to HTML using the [Markdig](https://github.com/lunet-io/markdig) library* with most of the advanced features turned on (except for Emoji, SoftLine as HardLine and SmartyPants).
* This extension has been tested with WordPress versions 4.5.2 and 4.5.3.
* This extension currently works with Git repos only (within VSTS). Support for Team Foundation Version Control (TFVC) will be added in a future release.

## Feedback and Support
This is an initial preview release of this extension. There are a few kinks yet to be worked out and features to be added. That said, if you like this extension, please leave a review and rating. If you have any suggestions for additional features or notice anything not working correctly, please let us know.

## Contact Us
* [Follow us on Twitter](https://twitter.com/moonspacelabs)
* [Follow us on Facebook](https://www.facebook.com/MoonspaceLabs/)

*[Markdig License Information](https://raw.githubusercontent.com/lunet-io/markdig/master/license.txt) 
