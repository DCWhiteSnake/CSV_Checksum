
# How To Use
CSV_Checksum converts a csv file to CHIP-0007 compatable jsons, calcalutes the SHA-256 hash for each new json file, and then appends the hash value to the original csv. This is a .Net6 app.

dotnet app.dll {pathToFolder} {filename.csv}

Example:
Let below be a sample of your filesystem
C:
├───SpecialFolder
│   └───filename.csv

Then in the console
dotnet app.dll C:\\SpecialFolder filename.csv
