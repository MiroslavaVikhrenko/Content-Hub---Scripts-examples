// User sign-in script

/*
 Important
Do not use this example on your own Content Hub instance without making changes to reflect your 
specific user groups.

The following script example is executed when a user logs into Sitecore Content Hub. 
It updates the user’s groups based on the provided claims.

Before you begin
    - Ensure claims mapping configurations for sign-in are set up in the schema editor.
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// The script ensures that user groups are updated for a user based on external claims if
// the authentication source is external.
if (Context.AuthenticationSource != AuthenticationSource.External)
{
    return;
}

// A list of group names is created to collect group information from external claims.
// If no claims are available, the script adds the default Everyone group.

List string groups = new List string();
if (Context.ExternalUserInfo?.Claims == null)
{
    groups.Add(Stylelabs.M.Sdk.Constants.UserGroup.Groups.Everyone);
}
else
{
    foreach (Stylelabs.M.Scripting.Types.V1_0.User.Claim group in Context.ExternalUserInfo.Claims)
    {
        if (group.Type == "MySpecialGroupType")
        {
            groups.Add(group.Value);
        }
    }
}

// The script retrieves the IDs of the collected user groups using the GetUserGroupIdsAsync method.
// These IDs will be used to update the user's group relations.

List long groupIds = (await MClient.Users.GetUserGroupIdsAsync(groups).ConfigureAwait(false))?.Values.ToList();

// The target user entity is retrieved from the context, and the UserGroupToUser relation is loaded.
// This relation represents the user's membership in specific groups.

var user = Context.User;

// ensure UserGroupToUser relation is loaded
await user.LoadMembersAsync(null, new RelationLoadOption("UserGroupToUser")).ConfigureAwait(false);

// The UserGroupToUser relation is updated with the IDs of the collected groups,
// linking the user to the appropriate groups.

user.GetRelation("UserGroupToUser").SetIds(groupIds);

// Finally, the user entity is saved, persisting the updated group memberships.
await MClient.Entities.SaveAsync(user).ConfigureAwait(false);


/*
 Setup
    - Create, publish, and enable the User sign-in script.
 */

/*
 Disable a script
    - A sign-in script might lock users out if it contains runtime errors or inconsistent user validation. 
If this happens, disable the script by using the REST API or SDK to change M.Script.Enabled to false.

The following example disables a script using the web SDK:
 */

var loadConfig = new EntityLoadConfiguration(
        CultureLoadOption.None,
        new PropertyLoadOption(
          ScriptingConstants.Scripting.Properties.Enabled),
        RelationLoadOption.None);
var script = await MClient.Entities.GetAsync(your script id, loadConfig).ConfigureAwait(false);
script?.SetPropertyValue("M.Script.Enabled", false);
await MClient.Entities.SaveAsync(script).ConfigureAwait(false);

