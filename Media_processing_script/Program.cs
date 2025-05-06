// Media processing script

/*
 This is an example of a Media processing script to be executed every time an asset is 
processed by the respective worker. It extracts the metadata properties from the context 
and adds them to the asset.
 */

using System.Linq;

// The script gets the MasterFile relation of the asset.

var masterFileRelation = await Context.File.GetRelationAsync<IChildToManyParentsRelation>("MasterFile");

// After that, it checks if the current file is the master file.
// Only this metadata is to be copied if this is the case.

if (!masterFileRelation.Parents.Any() || !masterFileRelation.Parents.Contains(Context.Asset.Id.Value))
{
    return;
}

// Next, it creates a method that checks whether the property already has commas and
// escapes them with quotation marks.

string ToCsvValue(object source)
{
    var str = source.ToString();
    if (str.Contains(","))
    {
        return "\"" + str + "\"";
    }
    return str;
}

// The script then converts the metadata headers to csv.

var headers = string.Join(", ", Context.MetadataProperties.Keys.Select(ToCsvValue));

// It also converts the metadata values to csv.

var values = string.Join(", ", Context.MetadataProperties.Values.Select(ToCsvValue));

// The script gets the metadata property field of the asset.

var metadataProp = await Context.Asset.GetPropertyAsync<ICultureInsensitiveProperty>("Metadata");

// Afterwards, it stores the metadata on the asset.

metadataProp.SetValue(headers + "\n" + values);

// At the end, the script saves the asset.

await MClient.Entities.SaveAsync(Context.Asset);

/*
 Setup
    - Create a string property called Metadata on the M.Asset definition, 
    setting the content type to Multiple lines.
    - Create, publish, and enable a metadata processing script.
 */