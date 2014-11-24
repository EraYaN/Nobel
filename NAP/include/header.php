<?php
require 'dbconnect.php';
require 'defines.php';

echo "<!DOCTYPE html>\n";
echo "<html lang='nl'>\n";

echo "<head>\n";

echo "<meta charset='utf-8'>\n";
echo "<title>joostwooning.nl</title>\n";

echo "<link rel='shortcut icon' href='images/logo.png'>\n";

echo "<link rel='stylesheet' type='text/css' href='css/normalize.css'>\n";
echo "<link rel='stylesheet' type='text/css' href='css/style.css'>\n";

echo "<script src='//ajax.googleapis.com/ajax/libs/jquery/2.1.1/jquery.min.js'></script>";

if (isset($htExtraHeaders) && is_array($htExtraHeaders)) {
	foreach ($htExtraHeaders as $htExtraHeader) {
		echo $htExtraHeader."\n";
	}
}

echo "</head>\n";

echo "<body>\n";

// --- sidebar ---

echo "<div id='sidebar'>\n";

echo "<h2>Overzicht</h2>\n";
echo "<a href='index.php'>Saldo</a>\n";
echo "<a href='overzicht_barbon.php'>Barbon</a>\n";

if (isPaltsgraaf()) {
	echo "<h2>Paltsgraaf</h2>\n";
	echo "<a href='invoer_barbon.php'>Invoer barbon</a>\n";
	echo "<a href='in_contributie.php'>In contributie</a>\n";
	echo "<a href='grootboek_bijwerken.php'>Grootboek bijwerken</a>\n";
	echo "<a href='bankboek_bijwerken.php'>Bankboek bijwerken</a>\n";
	echo "<a href='in_spaarplan.php'>In spaarplan</a>\n";
}

echo "</div>\n";
echo "<div id='content_wrapper'>\n";
?>