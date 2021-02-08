<html>
<link rel="stylesheet" href="style.css">

<script type="application/ld+json">

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

</script>

<script>
function openCoordinatesInGoogleMaps(y,x){
	window.open("https://www.google.com/maps/search/?api=1&query="+x+","+y);
}
</script>

<h1> Manual People Counters In Central Park </h1>
<p> Click a row to view its location in google maps</p>

<table name = "features">
	<tr>
		<th>Gate Name</th>
		<th>Age</th>
		<th>Activity</th>
		<th>Gender</th>
		<th>Location</th>
	</tr>
<?php 

$jsonLDObj = json_decode($jsonLD);

foreach($jsonLDObj->features as $feature){

$properties = $feature->properties;
$coordinates = $feature->geometry->coordinates;
echo '<tr onClick="openCoordinatesInGoogleMaps(' . $coordinates[0] . ',' .$coordinates[1] .')">';

echo '<td>' . $properties->{'Gate Name'} . '</td>';
echo '<td>' . $properties->{'Age'} . '</td>';
echo '<td>' . $properties->{'Activity'} . '</td>';
echo '<td>' . $properties->{'Gender'} . '</td>';


echo '<td> ('.$coordinates[1].','.$coordinates[0].')</td>';
//echo '<td>' . $geometry->coordinates[0] . '</td>';
//echo '<td>' . $geometry->coordinates[1] . '</td>';

echo '</tr>';

}
?>

</table>
</html>