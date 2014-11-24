<?
require 'nobellib.php';

$htExtraHeaders = array();
$htExtraHeaders[] = "<script src='js/invoer_barbon.js'></script>";
require 'header.php';

if (!isPaltsgraaf()) {
	echo "<p>U bent niet gemachtigd om deze pagina te bekijken.</p>\n";
	require 'footer.php';
	exit();
}

if (isset($_POST['invoer_barbon'])) {
	if (isset($_POST['nakijken_barbon'])) {
		nob_assert(isset($_POST['datum']), 'datum is niet gezet', true);
		nob_assert(isset($_POST['crediteur']), 'crediteur is niet gezet', true);
		nob_assert(isset($_POST['nummer']), 'nummer is niet gezet', true);
		nob_assert(isset($_POST['ho']), 'ho is niet gezet', true);
		nob_assert(isset($_POST['totaalbedrag']), 'totaalbedrag is niet gezet', true);
		nob_assert(isset($_POST['weging']), 'weging is niet gezet', true);
		nob_assert(isset($_POST['persoonlijk']), 'persoonlijk is niet gezet', true);
		nob_assert(is_array($_POST['weging']), 'weging is geen array', true);
		nob_assert(is_array($_POST['persoonlijk']), 'persoonlijk is geen array', true);

		$iTotaalbedrag = (int)round($_POST['totaalbedrag']/DATABASE_CURRENCY_PRECISION);
		$iHO = (int)round($_POST['ho']/DATABASE_CURRENCY_PRECISION);
		$iPersoonlijk = array();
		foreach ($_POST['persoonlijk'] as $id => $value) {
			$iPersoonlijk[(int)$id] = (int)round($value/DATABASE_CURRENCY_PRECISION);
		}

		nob_assert(array_sum($iPersoonlijk) + $iHO == $iTotaalbedrag, 'Som klopt niet', true);

		$res = nob_exec_query("SELECT `id` FROM `gb_barbon` WHERE `nummer`=".nob_quotevalue($_POST['nummer']));
		nob_assert($res !== false, 'Kon nog kijken of barbonnummer al bestaat', true);
		nob_assert($res->num_rows == 0, 'Barbonnummer bestaat al in database', true);

		$res = nob_exec_query("SELECT `id` FROM `crediteur` WHERE `nummer`=".(int)$_POST['crediteur']);
		nob_assert($res !== false, 'Kon niet kijken of crediteur bestaat', true);
		nob_assert($res->num_rows == 1, 'Crediteur bestaat niet in database', true);

		$mysqli->autocommit(false);

		$iTotalWeging = (int)array_sum($_POST['weging']);
		$iDeelHO = array();
		foreach ($_POST['weging'] as $id => $weging) {
			$deelHO = round(((int)$weging/$iTotalWeging)*$iHO);
			if ($deelHO != 0) {
				$iDeelHO[(int)$id] = $deelHO;
			}
		}

		// make sure that the total is the actual total by randomly adding numbers to ho values 
		while (array_sum($iDeelHO) != $iHO) {
			$iDeelHO[array_rand($iDeelHO)] += (array_sum($iDeelHO) < $iHO ? 1 : -1);
		}

		nob_assert($iTotaalbedrag == array_sum($iDeelHO) + array_sum($iPersoonlijk), 
		             'ongeldig totaalbedrag, klopt niet met individuele waarden', 
		             true);

		foreach ($_POST['weging'] as $id => $weging) {
			$iID = (int)$id;
			nob_assert(isset($iPersoonlijk[$iID]), 'persoonlijk is niet gezet', true);

			$creditDebit = getCreditDebitNummer($iID);

			if ($iPersoonlijk[$iID] != 0) {
				$iCredDeb = (int)($iPersoonlijk[$iID] > 0 ? $creditDebit['debit'] : $creditDebit['credit']);
				$qtBedrag = round(abs($iPersoonlijk[$iID] * DATABASE_CURRENCY_PRECISION), 4);
				$res = nob_exec_query("INSERT INTO `gb_barbon`".
				                  " SET `datum`=".nob_quotevalue(date('Y-m-d', strtotime($_POST['datum']))).",".
				                      " `nummer`=".nob_quotevalue($_POST['nummer']).",".
				                      " `cred/debnummer`={$iCredDeb},".
				                      " `bedrag`={$qtBedrag},".
				                      " `opmerking`=".nob_quotevalue($_POST['opmerking']).",".
				                      " `persoonlijk/ho`='persoonlijk'");
				nob_assert($res !== false, 'kon geen persoonlijke waarde invoeren', true);
			}

			if (isset($iDeelHO[$id]) && $iDeelHO[$id] != 0) {
				$iCredDeb = (int)($iDeelHO[$id] > 0 ? $creditDebit['debit'] : $creditDebit['credit']);
				$qtBedrag = round(abs($iDeelHO[$id] * DATABASE_CURRENCY_PRECISION), 4);
				$res = nob_exec_query("INSERT INTO `gb_barbon`".
				                  " SET `datum`=".nob_quotevalue(date('Y-m-d', strtotime($_POST['datum']))).",".
				                      " `nummer`=".nob_quotevalue($_POST['nummer']).",".
				                      " `cred/debnummer`={$iCredDeb},".
				                      " `bedrag`={$qtBedrag},".
				                      " `opmerking`=".nob_quotevalue($_POST['opmerking']).",".
				                      " `persoonlijk/ho`='ho',".
				                      " `weging`=".(int)$weging);
				nob_assert($res !== false, 'kon geen ho deel invoeren', true);
			}

			$iTotal = $iPersoonlijk[$iID] + (isset($iDeelHO[$id]) ? $iDeelHO[$id] : 0);
			if ($iTotal != 0) {
				$qtCredDeb = ($iTotal > 0 ? 'deb' : 'cred');
				$iCredDeb = (int)($iTotal > 0 ? $creditDebit['debit'] : $creditDebit['credit']);
				$qtBedrag = round(abs($iTotal * DATABASE_CURRENCY_PRECISION), 4);
				$res = nob_exec_query("INSERT INTO `gb_{$qtCredDeb}iteur`".
				                  " SET `datum`=".nob_quotevalue(date('Y-m-d', strtotime($_POST['datum']))).",".
				                      " `factuurnummer`=".nob_quotevalue($_POST['nummer']).",".
				                      " `grootboekrekeningnummer`=".(int)GB_BARBON_NUMMER.",".
				                      " `bedrag`={$qtBedrag},".
				                      " `uitleg`=".nob_quotevalue($_POST['opmerking']).",".
				                      " `{$qtCredDeb}iteurnummer`={$iCredDeb}");
				nob_assert($res !== false, 'kon geen cred/debiteur invoeren', true);
			}
		}

		if ($iTotaalbedrag != 0) {
			nob_assert(floor($_POST['crediteur'] / 10000) == 1, 'Ongeldig crediteurnummer', true);
			$iTotaalPersoonlijk = array_sum($iPersoonlijk);
			if ($iTotaalPersoonlijk != 0) {
				$iCredDeb = (int)($iTotaalPersoonlijk > 0 ? $_POST['crediteur'] : $_POST['crediteur'] + 10000);
				$qtBedrag = round(abs($iTotaalPersoonlijk * DATABASE_CURRENCY_PRECISION), 4);
				$res = nob_exec_query("INSERT INTO `gb_barbon`".
				                  " SET `datum`=".nob_quotevalue(date('Y-m-d', strtotime($_POST['datum']))).",".
					                      " `nummer`=".nob_quotevalue($_POST['nummer']).",".
					                      " `cred/debnummer`={$iCredDeb},".
					                      " `bedrag`={$qtBedrag},".
					                      " `opmerking`=".nob_quotevalue($_POST['opmerking']).",".
					                      " `persoonlijk/ho`='persoonlijk'");
				nob_assert($res !== false, 'kon geen totaal persoonlijk maken', true);
			}

			if ($iHO != 0) {
				$iCredDeb = (int)($iHO > 0 ? $_POST['crediteur'] : $_POST['crediteur'] + 10000);
				$qtBedrag = round(abs($iHO * DATABASE_CURRENCY_PRECISION), 4);
				$res = nob_exec_query("INSERT INTO `gb_barbon`".
				                  " SET `datum`=".nob_quotevalue(date('Y-m-d', strtotime($_POST['datum']))).",".
					                      " `nummer`=".nob_quotevalue($_POST['nummer']).",".
					                      " `cred/debnummer`={$iCredDeb},".
					                      " `bedrag`={$qtBedrag},".
					                      " `opmerking`=".nob_quotevalue($_POST['opmerking']).",".
					                      " `persoonlijk/ho`='ho'");
				nob_assert($res !== false, 'kon geen totaal ho maken', true);
			}

			$qtCredDeb = ($iTotaalbedrag > 0 ? 'cred' : 'deb');
			$iCredDeb = (int)($iTotaalbedrag > 0 ? $_POST['crediteur'] : $_POST['crediteur'] + 10000);
			$qtBedrag = round(abs($iTotaalbedrag * DATABASE_CURRENCY_PRECISION), 4);
			$res = nob_exec_query("INSERT INTO `gb_{$qtCredDeb}iteur`".
			                  " SET `datum`=".nob_quotevalue(date('Y-m-d', strtotime($_POST['datum']))).",".
			                      " `factuurnummer`=".nob_quotevalue($_POST['nummer']).",".
			                      " `grootboekrekeningnummer`=".(int)GB_BARBON_NUMMER.",".
			                      " `bedrag`={$qtBedrag},".
			                      " `uitleg`=".nob_quotevalue($_POST['opmerking']).",".
			                      " `{$qtCredDeb}iteurnummer`={$iCredDeb}");
			nob_assert($res !== false, 'kon geen cred/debiteur maken voor cred/debiteur', true);
		}

		$mysqli->commit();
		$mysqli->autocommit(true);

		nob_redirect('invoer_barbon.php');
	} else {
		echo "<h1>Nakijken barbon</h1>\n";

		$res = nob_exec_query("SELECT `id` FROM `gb_barbon` WHERE `nummer`=".nob_quotevalue($_POST['nummer']));
		nob_assert($res !== false, 'Kon nog kijken of barbonnummer al bestaat', true);
		if ($res->num_rows > 0) {
			echo "<p>Barbonnummer bestaat al in database</p><a href='' onclick='history.go(-1);'>Ga terug</a>\n";
			require 'footer.php';
			exit();
		}

		$totalWeging = array_sum($_POST['weging']);

		echo "<form action='".htmlentities($_SERVER['SCRIPT_NAME'])."' method='post' id='form_nakijk'>\n";
		echoHiddenInput($_POST);

		echo "<table>\n";

		echo "<tr>\n";
		echo "<td>Datum: </td>\n";
		echo "<td>".htmlentities(date('l d F Y', strtotime($_POST['datum'])))."</td>\n";
		echo "</tr>\n";

		echo "<tr>\n";
		echo "<td>Crediteur:</td>\n";
		$res = nob_exec_query("SELECT `naam` FROM `crediteur` WHERE `nummer`=".(int)$_POST['crediteur']);
		nob_assert($res !== false, 'Kon geen crediteuren opzoeken', true);
		nob_assert($res->num_rows == 1, 'Ongeldig crediteurnummer', true);
		$row = $res->fetch_assoc();
		echo "<td>".htmlentities($row['naam'])." (".(int)$_POST['crediteur'].")</td>\n";
		echo "</tr>\n";

		echo "<tr>\n";
		echo "<td>Nummer:</td>\n";
		echo "<td>".htmlentities($_POST['nummer'])."</td>\n";
		echo "</tr>\n";

		if (isset($_POST['opmerking'])) {
			echo "<tr>\n";
			echo "<td>Opmerking:</td>\n";
			echo "<td>".htmlentities($_POST['opmerking'])."</td>\n";
			echo "</tr>\n";
		}

		echo "</table>\n";
		echo "<hr>\n";
		echo "<table class='data'>\n";

		echo "<tr>\n";
		echo "<td>Naam</td>\n";
		echo "<td>Persoonlijk</td>\n";
		echo "<td>Weging</td>\n";
		echo "<td>Deel HO</td>\n";
		echo "<td>Totaal</td>\n";
		echo "</tr>\n";

		foreach ($_POST['weging'] as $id => $value) {
			$deelHO = ($_POST['weging'][$id]/$totalWeging)*$_POST['ho'];
			echo "<tr>\n";
			echo "<td>".htmlentities($_POST['naam'][$id])."</td>\n";
			echo "<td>".nob_htEuroValue($_POST['persoonlijk'][$id])."</td>\n";
			echo "<td>".(int)$_POST['weging'][$id]."</td>\n";
			echo "<td>".nob_htEuroValue($deelHO)."</td>\n";
			echo "<td>".nob_htEuroValue($deelHO + $_POST['persoonlijk'][$id])."</td>\n";
			echo "</tr>\n";
		}

		echo "<tr class='table_sum'>\n";
		echo "<td>&nbsp;</td>\n";
		echo "<td>".nob_htEuroValue(array_sum($_POST['persoonlijk']))."</td>\n";
		echo "<td>&nbsp;</td>\n";
		echo "<td>".nob_htEuroValue($_POST['ho'])."</td>\n";
		echo "<td>".nob_htEuroValue($_POST['totaalbedrag'])."</td>\n";
		echo "</tr>\n";

		echo "</table>\n";
		echo "<input type='submit' name='nakijken_barbon'>\n";
		echo "</form>\n";

		require 'footer.php';
		exit();
	}
}

echo "<h1>Invoer barbon</h1>\n";

echo "<form action='".htmlentities($_SERVER['SCRIPT_NAME'])."' method='post' id='form_invoer'>\n";
echo "<table>\n";

echo "<tr>\n";
echo "<td>Datum: </td>\n";
echo "<td><input required type='date' name='datum'></td>\n";
echo "</tr>\n";

echo "<tr>\n";
echo "<td>Crediteur: </td>\n";
echo "<td>\n";
echo "<select required name='crediteur'>\n";
$res = nob_exec_query("SELECT `nummer`, `naam`".
                  " FROM `crediteur`".
                  " WHERE `lid_id` IS NULL".
                  " ORDER BY `nummer`");
nob_assert($res !== false, 'Kon geen crediteuren opzoeken', true);
while (null !== ($row = $res->fetch_assoc())) {
	$iNummer = (int)$row['nummer'];
	echo "<option value='{$iNummer}'".($iNummer == DSB_CREDIT_NUMMER_DEBITEUREN ? 'selected' : '').">".
	     htmlentities($row['naam'])."</option>\n";
}
echo "</select>\n";
echo "</td>\n";
echo "</tr>\n";

echo "<tr>\n";
echo "<td>Nummer: </td>\n";
echo "<td><input required type='text' name='nummer'></td>\n";
echo "</tr>\n";

echo "<tr>\n";
echo "<td>Totaalbedrag: </td>\n";
echo "<td>&euro;<input required type='number' step='".DATABASE_CURRENCY_PRECISION."' name='totaalbedrag'".
     " id='totaalbedrag'></td>\n";
echo "</tr>\n";

echo "<tr>\n";
echo "<td>HO: </td>\n";
echo "<td>&euro;<input required type='number' step='".DATABASE_CURRENCY_PRECISION."' name='ho' id='ho'></td>\n";
echo "</tr>\n";

echo "<tr>\n";
echo "<td>Opmerking: </td>\n";
echo "<td><input type='text' name='opmerking'></td>\n";
echo "</tr>\n";

echo "</table>\n";

echo "<hr>\n";

echo "<table class='data'>\n";

echo "<tr>\n";
echo "<td>Naam</td>\n";
echo "<td>Persoonlijk</td>\n";
echo "<td>Weging</td>\n";
echo "</tr>\n";

$res = nob_exec_query("SELECT `id`, `voornaam`, `tussenvoegsel`, `achternaam`, `status`, `is_bestuur`, `is_commissie`".
                  " FROM `lid`".
                  " WHERE `status`<>'Oud-lid'");
nob_assert($res !== false, 'Could not look up users', true);

$iIndex = 1000; //random amout of input field before table
$iAmount = 1000; //random amount of users
while (null !== ($row = $res->fetch_assoc())) {
	$iID = (int)$row['id'];
	$htNaam = htmlentities($row['voornaam']." ".$row['tussenvoegsel']." ".$row['achternaam']);
	echo "<tr>\n";
	echo "<input type='hidden' name='naam[{$iID}]' value='{$htNaam}'>\n";
	echo "<td>{$htNaam}</td>\n";
	echo "<td>&euro;<input type='number' name='persoonlijk[{$iID}]' class='small input_persoonlijk'".
	     " onclick='this.select();' tabindex='{$iIndex}' id='persoonlijk_{$iID}'".
	     " step='".DATABASE_CURRENCY_PRECISION."'></td>\n";
	
	$iStdWeging = (int)STD_WEGING_LID;
	if ($row['is_bestuur'] == 1) {
		$iStdWeging = (int)STD_WEGING_BESTUUR;
	}
	if ($row['is_commissie'] == 1) {
		$iStdWeging = (int)STD_WEGING_COMMISSIE;
	}
	if ($row['status'] == 'Externe Orde') {
		$iStdWeging = (int)STD_WEGING_EXTERNE_ORDE;
	}

	echo "<td><input type='number' min='0' step='1' name='weging[{$iID}]' id='weging_{$iID}'".
	               " class='small input_weging' value='{$iStdWeging}'".
	               " onclick='this.select();' tabindex='".($iIndex + $iAmount)."' required>";
	foreach (array(0, 3, 5, 7, 10) as $i) {
		echo " <a href='' onclick='document.getElementById(\"weging_{$iID}\").value=\"{$i}\"; return false;'>{$i}</a>";
	}
	echo "</td>\n";
	
	echo "</tr>\n";

	$iIndex++;
}

echo "</table>\n";
echo "<input type='submit' name='invoer_barbon'>\n";
echo "</form>\n";

require 'footer.php';

// joost20141015 print een hidden input veld, met als name de key van array en value is de waarde bij de key.
// IN:  - data        key value array welke geprint wordt
//      - namePrefix  string welke voor de name wordt geprint
//      - nameSuffix  string welke achter de name wordt geprint
function echoHiddenInput($data, $namePrefix='', $nameSuffix='') {
	nob_assert(is_array($data), 'Data should be an array', true);

	foreach ($data as $name => $value) {
		if (is_array($value)) {
			echoHiddenInput($value, $name.'[', ']');
		} else {
			echo "<input type='hidden' name='".htmlentities($namePrefix.$name.$nameSuffix)."'".
			           " value='".htmlentities($value)."'>\n";
		}
	}
}
?>