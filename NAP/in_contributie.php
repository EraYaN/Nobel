<?
require 'nobellib.php';
require 'header.php';

if (!isPaltsgraaf()) {
	echo "<p>U bent niet gemachtigd om deze pagina te bekijken.</p>\n";
	require 'footer.php';
	exit();
}

if (isset($_POST['in_contributie'])) {
	nob_assert(isset($_POST['datum']), 'datum is niet gezet', true);
	nob_assert(isset($_POST['omschrijving']), 'omschrijving is niet gezet', true);
	nob_assert(isset($_POST['contributie']), 'contributie is niet gezet', true);
	nob_assert(is_array($_POST['contributie']), 'contributie is geen array', true);
	nob_assert(strtotime($_POST['omschrijving']) !== false, 'omschrijving moet "maand jaar" zijn', true);

	$mysqli->autocommit(false);

	foreach ($_POST['contributie'] as $ID => $contributie) {
		$iID = (int)$ID;

		$creditDebit = getCreditDebitNummer($iID);

		if ($_POST['contributie'][$iID] != 0) {
			$res = nob_exec_query("INSERT INTO `gb_contributie`".
			                     " SET `datum`=".nob_quotevalue(date('Y-m-d', strtotime($_POST['datum']))).",".
			                         " `omschrijving`=".nob_quotevalue($_POST['omschrijving']).",".
			                         " `bedrag`=".abs((float)$contributie).",".
			                         " `debiteurnummer`=".(int)$creditDebit['debit']);
			nob_assert($res !== false, 'kon geen contributie invoeren voor {$iID}', true);

			$qtFactuurnummer = nob_quotevalue('CB'.date('mY', strtotime($_POST['omschrijving'])));
			$res = nob_exec_query("INSERT INTO `gb_debiteur`".
			                     " SET `datum`=".nob_quotevalue(date('Y-m-d', strtotime($_POST['datum']))).",".
			                         " `factuurnummer`={$qtFactuurnummer},".
			                         " `grootboekrekeningnummer`=".(int)GB_CONTRIBUTIE_NUMMER.",".
			                         " `bedrag`=".abs((float)$contributie).",".
			                         " `uitleg`=".nob_quotevalue($_POST['omschrijving']).",".
			                         " `debiteurnummer`=".(int)$creditDebit['debit']);
			nob_assert($res !== false, 'kon geen debiteur aanmaken voor {$iID} (contributie)', true);
		}
	}

	$mysqli->commit();
	$mysqli->autocommit(true);

	nob_redirect('in_contributie.php');
}

echo "<h1>In contributie</h1>\n";

$res = nob_exec_query("SELECT DISTINCT `omschrijving` FROM `gb_contributie` ORDER BY `datum` LIMIT 5");
nob_assert($res !== false, 'Kon geen vijf meest recente contributies innen', true);
echo "<p>Vijf meest recente inningen: ";
while (null !== ($row = $res->fetch_assoc())) {
	echo htmlentities($row['omschrijving']).", ";
}
echo "</p>\n";

echo "<form action='".htmlentities($_SERVER['SCRIPT_NAME'])."' method='post' id='form_contributie'>\n";

echo "<table>\n";

echo "<tr>\n";
echo "<td>Datum: </td>\n";
echo "<td><input type='hidden' name='datum' value='".htmlentities(date('Y-m-d'))."'>".
     htmlentities(date('l d F Y'))."</td>\n";
echo "</tr>\n";

echo "<tr>\n";
echo "<td>Omschrijving:</td>\n";
echo "<td><input type='text' name='omschrijving' value='".htmlentities(date('F Y'))."'></td>\n";
echo "</tr>\n";

echo "</table>\n";
echo "<hr>\n";
echo "<table class='data'>\n";

$htOddEven = 'even';

echo "<tr>\n";
echo "<td>Naam</td>\n";
echo "<td>Contributie</td>\n";
echo "</tr>\n";

$res = nob_exec_query("SELECT `id`, `voornaam`, `tussenvoegsel`, `achternaam`, `status`, `is_bestuur`".
                     " FROM `lid`".
                     " WHERE `status`<>'Oud-lid'");
nob_assert($res !== false, 'Kon geen leden opzoeken', true);
while (null !== ($row = $res->fetch_assoc())) {
	echo "<tr>\n";
	echo "<td>".htmlentities($row['voornaam']." ".$row['tussenvoegsel']." ".$row['achternaam'])."</td>\n";
	$fContributie = (float)CONTRIBUTIE_LID;
	if ($row['is_bestuur'] == 1) {
		$fContributie = (float)CONTRIBUTIE_BESTUUR;
	}
	if ($row['status'] == 'Externe Orde') {
		$fContributie = (float)CONTRIBUTIE_EXTERNE_ORDE;	
	}
	echo "<td><input type='hidden' name='contributie[".(int)$row['id']."]' value='{$fContributie}'>".
	     nob_htEuroValue($fContributie)."</td>\n";
	echo "</tr>\n";
}

echo "</table>\n";
echo "<input type='submit' name='in_contributie'>\n";
echo "</form>\n";

require 'footer.php';
?>