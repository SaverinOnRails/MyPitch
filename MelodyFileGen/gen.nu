# Must be run for MelodyGen folder
let manifest = open Midis/manifest.json
dotnet build --property WarningLevel=0
$manifest | transpose  name value | par-each { |f|
  ./bin/Debug/net10.0/MelodyFileGen run --property WarningLevel=0 -- --file ("Midis/" ++ $f.name ++ ".mid") --channel $f.value.Channel --tonic $f.value.Tonic | print $in
}
mv *.json ../MyPitch/FolkDatabase


