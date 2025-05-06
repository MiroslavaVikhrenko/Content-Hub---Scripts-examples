using System.Linq;
/*
 Action script with a pre-commit trigger

This example script demonstrates how to automatically validate or modify entity data before it's saved, 
ensuring that all required fields are complete and compliant with your business rules. 
The script is connected to a pre-commit trigger that executes before applied changes are saved by 
the system. For example, when uploading assets in bulk, this script ensures that images and web-friendly 
file types are appropriately categorized without manual intervention.

Before you begin
    >> Ensure the assets referenced in the script exist in the Content Hub schema.
    >> Define any required properties, such as Title or Description, used in the script logic.
    >> Verify that users or roles executing the script have the necessary permissions to create 
or modify the referenced entities.
 */

// The script defines a list of web-compatible file extensions to determine which
// files qualify as web assets.
var webExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

// The target entity is retrieved from the context (Context.Target) and loaded with
// the necessary members. These include the FileName property and the AssetTypeToAsset relation.
// If the entity is invalid or the filename is empty, the script exits.

var entity = Context.Target as IEntity;
// Make sure that the following members are loaded
await entity.LoadMembersAsync(new PropertyLoadOption("FileName"), 
    new RelationLoadOption("AssetTypeToAsset"));
var filename = entity.GetPropertyValue<string>("FileName");
if (string.IsNullOrEmpty(filename)) return; // No filename, nothing to do.

/*
 Warning
The Target object type depends on the trigger's objectives 
(for example, entity, entity definition, or policy). 
In this case, the trigger's objectives are Entity creation and Entity modification. 
Hence, we need to cast Target to IEntity.
 */

// The file extension is extracted from the filename using the GetExtension helper method.
// If the extension is missing or not in the list of web-compatible extensions, the script exits.

var extension = GetExtension(filename)?.ToLowerInvariant();
if (string.IsNullOrEmpty(extension)) return;
if (!webExtensions.Contains(extension)) return;

// A query is created to find the M.AssetType.Web entity.
// If the entity is not found, the script exits.

var query = Query.CreateQuery(entities =>
  from e in entities
  where e.Identifier == "M.AssetType.Web"
  select e);
var webTypeId = await MClient.Querying.SingleIdAsync(query);
if (!webTypeId.HasValue) return;

/*
 Note
The MClient object is always available and can be used by all script types.
 */

// The script attempts to retrieve the AssetTypeToAsset relation from the asset.
// If the relation exists, it sets the parent of the relation to the M.AssetType.Web entity,
// linking the asset to the web asset type. If there is no relation, the script exits.

var relation = entity.GetRelation<IChildToOneParentRelation>("AssetTypeToAsset");
if (relation == null) return;
relation.Parent = webTypeId;

// The helper method GetExtension extracts the file extension from the filename,
// ensuring that only relevant file types are processed.
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
    - On the Taxonomy page, create an asset type named Web with the identifier M.AssetType.Web.
    - Create, publish, and enable the script above as an Action script.
    - Create an action of type Action script and link it to the script.
    - Create a new trigger and set the trigger's objectives to Entity creation and Entity modification.
    - In the trigger conditions, add the entity definition Asset then add the condition 
Filename has changed.
    - In the trigger actions, add the new action to Pre-commit actions.
    - Save and activate the trigger.
 */