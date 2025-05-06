// Action script with a validation trigger

/*
 This example script verifies that an asset satisfies specific criteria, 
such as having required fields or valid relationships, before allowing it to be saved or published. 
The script is connected to a validation trigger that checks data integrity before committing changes.

Before you begin
    - Ensure the asset or assets referenced in the script exist in the Content Hub schema.
    - Define any required properties, such as Title or Description, used in the script logic.
    - Verify that users or roles executing the script have the necessary permissions to create 
or modify the referenced entities.
 */

using System.Linq;
// The script defines a list of valid web extensions.
// If an asset's file extension is not in this list, then its filename is not considered valid.

var webExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

// The script then retrieves the Target object from the Context and casts it to IEntity.
// Target is the asset being created or modified.

var entity = Context.Target as IEntity;

/*
 The TargetType context property defines the type of Target, which can be Entity, 
EntityDefinition, Policy, or DataSource. The Target object type depends on the trigger's objectives, 
such as entity, entity definition, or policy. Since this trigger is for entity creation and entity 
modification, Target must be cast to IEntity.

It then retrieves the Filename property from the Target object using GetPropertyValueAsync.
 */

var filename = await entity.GetPropertyValueAsync<string>("FileName");

/*
 Note
The scripting API uses lazy loading. GetPropertyValueAsync checks whether the property is already loaded. 
If it's not, the script will load it for the user.
 */

// If the asset doesn't have a filename, the script exits.

if (string.IsNullOrEmpty(filename)) return;

// The script then gets the extension from the filename using Path.GetExtension (System.IO).

var extension = GetExtension(filename)?.ToLowerInvariant();

// If the filename does not have an extension, the script exits.

if (string.IsNullOrEmpty(extension)) return;

// If the extension is not a valid web extension, the script throws a ValidationException.

if (!webExtensions.Contains(extension))
{
    throw new ValidationException(
      "The asset is not valid.",
      new ValidationFailure("The file's extension must be the extension of a valid web filetype.", filename));
}

// The script extracts the file extension from the file path. It splits the path by periods (.)
// and returns the last segment prefixed with a dot (.) if there are multiple segments.
// If no extension is found, it returns null.

string GetExtension(string path)
{
    var tokens = path.Split('.');
    if (tokens.Length > 1)
    {
        return "." + tokens[tokens.Length - 1];
    }
    return null;
}

/*
 Setup
    - Create, publish, and enable an Action script.
    - Create an action of type Action script and link it with the script.
    - Create a new trigger and set the trigger's objective to Entity creation and Entity modification.
    - In the trigger conditions, add the entity definition Asset then add the condition 
Filename has changed.
    - In the trigger actions, add the new action under Validation actions.
    - Save and activate the trigger.
 */