// Action script with a security trigger

/*
 This example script demonstrates how to enforce custom security policies by automatically restricting 
or granting access to specific entities or properties based on a user's roles and permissions. 
The script is connected to a security trigger that enforces access control by validating user 
permissions before executing actions.

Before you begin
    - Confirm that security policies are defined for the entity types involved in the script, 
such as M.Asset or M.Content.
    - Ensure the schema includes properties required for security checks, such as Owner or UserGroup.
    - Test the permissions of the user or role to validate the enforcement of security rules.
 */

//The script first creates an EntityLoadConfiguration object to specify which properties should be 
//loaded with the entity. In this example, we only need to load the UserGroupToUser relation, which 
//specifies which groups the user belongs to.

var loadConfig = new EntityLoadConfiguration
{
    CultureLoadOption = CultureLoadOption.None,
    RelationLoadOption = new RelationLoadOption("UserGroupToUser"),
    PropertyLoadOption = PropertyLoadOption.None
};

// Next, it retrieves the user entity by specifying the user's ID ( Context.TriggeringUserId.Value )
// and the load configuration.

var user = await MClient.Entities.GetAsync(Context.TriggeringUserId.Value, loadConfig);

// If no user object is returned, the script throws an InvalidOperationException.

if (user == null) throw new InvalidOperationException("Triggering user could not be found.");

// Next, it attempts to fetch the Web agency users group.
// If that group isn't found, it throws an InvalidOperationException.

var webAgencyGroup = await MClient.Users.GetUserGroupAsync("Web agency users");
if (webAgencyGroup == null) throw new InvalidOperationException("Web agency usergroup not found.");

// It then gets the user groups of the user is part of.

var userGroups = await user.GetRelationAsync<IChildToManyParentsRelation>("UserGroupToUser");

// Then the script checks if the current user belongs to webAgencyGroup before allowing them to create
// or modify certain assets. If the user is not in this group, the script throws a ForbiddenException.
// This means that only members of the Web agency user group can create or modify image-type web assets.

if (!userGroups.Parents.Contains(webAgencyGroup.Id.Value))
{
    throw new ForbiddenException("Only users of usergroup 'Web agency users' are allowed to create or modify assets of image-type 'Web'.");
}

/*
 Setup
    - Add a new class to the the M.AssetType taxonomy with the label Web.
    - Create, publish, and enable an Action script.
    - Create an action of type Action script and link it with the script.
    - Create a new trigger and set its objectives to Entity creation and Entity modification.
        - In the trigger conditions, add the entity definition Asset and add a new condition. 
        Set that condition to Type (AssetTypeToAsset) current value contains any Web.
        - In the trigger actions, add the new action under Security actions.
    - Save and activate the trigger.
 */