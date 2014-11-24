<?
require 'nobellib.php';
require 'header.php';

if (!isPaltsgraaf()) {
	echo "<p>U bent niet gemachtigd om deze pagina te bekijken.</p>\n";
	require 'footer.php';
	exit();
}

if (isset($_POST['bankboek_bijwerken'])) {
	foreach ($_POST['cred/debiteurnummer'] as $id => $nummer) {
		$iID = (int)$id;

		$qtNummer = (int)$nummer;
		if ($nummer == 0) {
			$qtNummer = 'NULL';
		}
		$res = nob_exec_query("UPDATE `gb_bankboek` SET `cred/debiteurnummer`={$qtNummer} WHERE `id`={$iID}");
		nob_assert($res !== false, 'Could not update cred/debiteurnummer', true);
	}

	foreach ($_POST['cred/debit'] as $id => $factuurnummer) {
		$iID = (int)$id;
		
		$qtNummer = nob_quotevalue($factuurnummer);
		if ($factuurnummer == 0) {
			$qtNummer = 'NULL';
		}
		$res = nob_exec_query("UPDATE `gb_bankboek` SET `cred/debit`={$qtNummer} WHERE `id`={$iID}");
		nob_assert($res !== false, 'Could not update cred/debiteurnummer', true);
	}

	nob_redirect('bankboek_bijwerken.php');
}

echo "<h1>Bankboek bijwerken</h1>\n";

echo "<h2>Ongekoppelde overboeking</h2>\n";
$res = nob_exec_query("SELECT `gb_bankboek`.`id`, `rekening`, `datum`, `transactiebedrag`, `gb_bankboek`.`type`,".
                            " `tegenrekening`, `gb_bankboek`.`naam`, `opmerking`, `cred/debiteurnummer`, `cred/debit`".
                     " FROM `gb_bankboek`".
                     " LEFT JOIN `crediteur`".
                      " ON `nummer`=`cred/debiteurnummer`".
                     " WHERE `cred/debiteurnummer`=0".
                     " OR (`crediteur`.`type`<>'Lid'".
                     " AND `cred/debit` IS NULL)".
                     " ORDER BY `datum` DESC");
nob_assert($res !== false, 'Could not select bankboek items', true);
displayBankboekTableForm($res);

echo "<hr>";
echo "<h2>Overboeking van de laatste maand</h2>\n";
$res = nob_exec_query("SELECT `id`, `rekening`, `datum`, `transactiebedrag`, `type`, `tegenrekening`, `naam`,".
                            " `opmerking`, `cred/debiteurnummer`, `cred/debit`".
                     " FROM `gb_bankboek`".
                     " WHERE `datum`>=".nob_quotevalue(date('Y-m-d', strtotime('-1 month'))).
                     " ORDER BY `datum` DESC");
nob_assert($res !== false, 'Could not select bankboek items', true);
displayBankboekTableForm($res);

require 'footer.php';

// joost20141022 functie welke een volledige tabel met forulier print, waarmee het bankboek bijgewerkt kan worden
// IN:  - mysqlResult  mysqli_result object waarover geloopt wordt, deze items zijn aan te passen. Moet uit de 
//                     tabel `gb_grootboek` zijn en de volgende waarden moeten geselecteerd worden:
//                     `id`, `rekening`, `datum`, `transactiebedrag`, `type`, `tegenrekening`, `naam`, `opmerking`, 
//                     `cred/debiteurnummer`, `cred/debit`
function displayBankboekTableForm($mysqlResult) {
	echo "<form action='".htmlentities($_SERVER['SCRIPT_NAME'])."' method='post'>\n";
	echo "<table class='data'>\n";

	echo "<tr>\n";
	echo "<td>Rekening</td>\n";
	echo "<td>Datum</td>\n";
	echo "<td>Bedrag</td>\n";
	echo "<td>Tegenrekening</td>\n";
	echo "<td>Type</td>\n";
	echo "<td>Naam</td>\n";
	echo "<td>Opmerking</td>\n";
	echo "<td>Cred/debiteur</td>\n";
	echo "<td>Cred/debit</td>\n";
	echo "</tr>\n";

	while (null !== ($row = $mysqlResult->fetch_assoc())) {
		echo "<tr>\n";
		echo "<td>".($row['rekening'] == 'lopend' ? 'Lopende ' : 'Spaar')."rekening</td>\n";
		echo "<td>".htmlentities(date('l d m Y', strtotime($row['datum'])))."</td>\n";
		echo "<td>".nob_htEuroValue($row['transactiebedrag'])."</td>\n";
		echo "<td>".htmlentities($row['tegenrekening'])."</td>\n";
		echo "<td>".htmlentities($row['type'])."</td>\n";
		echo "<td>".htmlentities($row['naam'])."</td>\n";
		echo "<td>".htmlentities($row['opmerking'])."</td>\n";

		$qtTableCredDeb = ((float)$row['transactiebedrag'] < 0  ? 'cred' : 'deb');
		$res2 = nob_exec_query("SELECT `nummer`, `naam`".
		                      " FROM `{$qtTableCredDeb}iteur`".
		                      " LEFT JOIN `lid`".
		                       " ON `lid`.`id`=`{$qtTableCredDeb}iteur`.`lid_id`".
		                      " WHERE `lid`.`status`<>'Oud-lid'".
		                      " OR `lid`.`id` IS NULL".
		                      " ORDER BY `nummer`");
		nob_assert($res2 !== false, 'Could not lookup name for cred/debiteur', true);
		
		echo "<td>\n";
		echo "<select name='cred/debiteurnummer[".(int)$row['id']."]'>\n";
		echo "<option value='0'></option>\n";
		while (null !== ($row2 = $res2->fetch_assoc())) {
			$iNummer = (int)$row2['nummer'];
			echo "<option".($iNummer == $row['cred/debiteurnummer'] ? ' selected' : '').
			     " value='{$iNummer}'>".htmlentities($row2['naam'])." ({$iNummer})</option>\n";
		}
		echo "</select>\n";
		echo "</td>\n";

		echo "<td>\n";
		$res2 = nob_exec_query("SELECT `type`".
		                      " FROM `{$qtTableCredDeb}iteur`".
		                      " WHERE `nummer`=".(int)$row['cred/debiteurnummer']);
		nob_assert($res2 !== false, 'Kon geen type cred/debiteur opzoeken', true);
		if ($res2->num_rows == 1) {
			$row2 = $res2->fetch_assoc();
			$isLid = ($row2['type'] == 'Lid');
		} else {
			$isLid = false;
		}

		if ($isLid) {
			echo 'BT';
		} else {
			echo "<select name='cred/debit[".(int)$row['id']."]'>\n";
			echo "<option value='0'></option>\n";

			$res2 = nob_exec_query("SELECT `factuurnummer`, `uitleg`".
			                      " FROM `gb_{$qtTableCredDeb}iteur`".
			                      " WHERE `{$qtTableCredDeb}iteurnummer`=".(int)$row['cred/debiteurnummer'].
			                      " AND `factuurnummer` NOT IN (".
			                        "SELECT `cred/debit`".
			                       " FROM `gb_bankboek`".
			                       " WHERE `cred/debiteurnummer`=".(int)$row['cred/debiteurnummer'].
			                       " AND `cred/debit` IS NOT NULL)".
			                      " OR `factuurnummer`=".nob_quotevalue($row['cred/debit']).
			                      " ORDER BY `factuurnummer`");
			nob_assert($res2 !== false, 'Could not lookup cred/debit', true);
			while (null !== ($row2 = $res2->fetch_assoc())) {
				echo "<option".($row2['factuurnummer'] == $row['cred/debit'] ? ' selected' : '').
				     " value='".htmlentities($row2['factuurnummer'])."'>".htmlentities($row2['uitleg']).
				     " (".htmlentities($row2['factuurnummer']).")</option>\n";
			}
			echo "</select>\n";
		}
		echo "</td>\n";

		echo "</tr>\n";
	}

	echo "</table>\n";
	echo "<br>\n";
	echo "<input type='submit' name='bankboek_bijwerken'>\n";
	echo "</form>\n";
}
?>