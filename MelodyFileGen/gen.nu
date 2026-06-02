# Must be run for MelodyGen folder
let manifest = open Midis/manifest.json
$manifest | transpose  name value | each { |f|
  dotnet run --property WarningLevel=0 -- --file ("Midis/" ++ $f.name ++ ".mid") --channel $f.value.Channel --tonic $f.value.Tonic | print $in
}
mv *.json ../MyPitch/FolkDatabase


