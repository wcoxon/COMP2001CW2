<?php 
$jsonData = file_get_contents('http://web.socem.plymouth.ac.uk/comp2001/wcoxon/dataset/index.json');
$geoJsonContext = file_get_contents("https://geojson.org/geojson-ld/geojson-context.jsonld");

function formatGeoJsonLD($contextJson,$datasetJson){

$contextObj = json_decode($contextJson)->{"@context"};
$datasetObj = json_decode($datasetJson);

$contextObj->{"Gate Name"} = "https://schema.org/name";
$contextObj->{"Age"} = "https://schema.org/description";
$contextObj->{"Activity"} = "https://schema.org/description";
$contextObj->{"Gender"} = "https://schema.org/gender";

$datasetObj->{"@context"} = $contextObj;

$formattedJsonLD = json_encode($datasetObj);

return $formattedJsonLD;

}

$jsonLD = formatGeoJsonLD($geoJsonContext,$jsonData);

echo $jsonLD;

?>