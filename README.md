# AadAppRoleManager
AAD AppRole Manager lets you view and edit App Role assignments for Applications registered in Azure Active Directory.

It is deployed at https://aadapprolemanager.azurewebsites.net for use. You must log in with a "Work or School" account, not a Microsoft Account, and that account must have administrator rights over the directory. Via OAuth, your account delegates access rights to this application to manipulate the directory on your behalf. When you're done, you can deauthorize the app by revoking its OAuth tokens at https://myapps.microsoft.com.

NB. If your account is in multiple AAD tenants, you will be delegating access to your home tenant, not to any of the others.

## Why?
Unfortunately at the time of writing, the Azure management portal does not allow you to manipulate App Role assignments for applications fully. You can assign a single App Role to a user, but not multiple. You can't assign an App Role to a group. This app lets you have full control.

## How quality is it?
This is not a polished application, and exists as a proof of concept and rough tool. There is basically no error handling, so if something goes wrong, you'll likely just get an error page.