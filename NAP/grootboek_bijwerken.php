<?
require 'nobellib.php';
require 'header.php';

if (!isPaltsgraaf()) {
	echo "<p>U bent niet gemachtigd om deze pagina te bekijken.</p>\n";
	require 'footer.php';
	exit();
}

$res = nob_exec_query("SELECT MAX(`datum`) FROM `gb_bankboek`");
nob_assert($res !== false, 'Kan geen meest recente datum vinden', true);
list($maxDate) = $res->fetch_row();

if (isset($_POST['grootboek_bijwerken'])) {
	nob_assert(isset($_FILES['file']), 'Geen bestand geupload', true);
	nob_assert($_FILES['file']['error'] == UPLOAD_ERR_OK, 'Er is een error opgetreden bij uploaden', true);

	$file = file($_FILES['file']['tmp_name']);

	$mysqli->autocommit(false);

	$htOddEven = 'even';
	foreach ($file as $line) {
		$htOddEven = ($htOddEven == 'even' ? 'odd' : 'even');

		$part = explode("\t", $line);
		nob_assert(count($part) == 8, 'Geen geldige lijn uit grootboek', true);

		$rekeningnummer    = $part[0]; // GEEN iban (dus ook zonder leading 0)
		$valuta            = $part[1]; // EUR (anders activeer PANIC MODUS)
		$transactieDatum   = $part[2]; // Ymd
		$fBeginSaldo       = (float)str_replace(',', '.', $part[3]);
		$fEindSaldo        = (float)str_replace(',', '.', $part[4]);
		$renteDatum        = $part[5]; // Ymd
		$fTransactiebedrag = (float)str_replace(',', '.', $part[6]); // bedrag naar ons (dus als negatief dan geld weg)
		$metaData          = $part[7]; // #blaat

		nob_assert($valuta == 'EUR', 'Geen euro als valuta PANIC MODUS', true);
		nob_assert($rekeningnummer == REKENING_NUMMER_LOPEND || $rekeningnummer == REKENING_NUMMER_SPAAR,
		           'Onbekende eigen rekening gevonden', 
		           true);

		$credDebiteurType = '';
		$credDebiteurNummer = 0;
		$credDebiteurNaam = '';
		$credDebiteurOmschrijving = '';

		// fix voor slashes split met debiteurennummer van nobel/barbonnummer
		$omschrijving = str_replace('MARF/700180/REMI', 'MARF-700180-REMI', $metaData);
		$omschrijving = str_replace('700180/', '700180-', $omschrijving);
		$omschrijving = str_replace('MARF-700180-REMI', 'MARF/700180/REMI', $omschrijving);
		$part = explode('/', $omschrijving);
		if (strpos($part[0], 'ABN AMRO Bank N.V.') === 0) {
			$credDebiteurNummer = 10000;
			$credDebiteurNaam = 'ABN AMRO Bank N.V.';
			$credDebiteurOmschrijving = 'Rekeningkosten';
			$tegenRekening = 'ABN';
		} else {
			array_shift($part);
			nob_assert(count($part) % 2 == 0, 'Ongeldige omschrijving', true);
			$omschrijving = array();
			$isKey = null;
			foreach ($part as $value) {
				if ($isKey == null) {
					$omschrijving[$value] = null;
					$isKey = $value;
				} else  {
					$omschrijving[$isKey] = $value;
					$isKey = null;
				}
			}

			nob_assert(isset($omschrijving['TRTP']), 'Type niet kunnen vinden', true);
			nob_assert(isset($omschrijving['IBAN']), 'Tegenrekening niet kunnen vinden', true);
			nob_assert(isset($omschrijving['NAME']), 'Naam niet kunnen vinden', true);

			$credDebiteurType = $omschrijving['TRTP'];

			$tegenRekening = $omschrijving['IBAN'];
			if (isset($omschrijving['REMI'])) {
				$credDebiteurOmschrijving = $omschrijving['REMI'];
			}

			if ($fTransactiebedrag < 0) {
				$qtTable = 'crediteur';
			} else {
				$qtTable = 'debiteur';
			}
			$res = nob_exec_query("SELECT `nummer`, `naam`".
			                     " FROM `{$qtTable}`".
			                     " WHERE `iban`=".nob_quotevalue($omschrijving['IBAN']));
			nob_assert($res !== false, 'Kon geen naam bij iban zoeken', true);
			if ($res->num_rows == 1) {
				$row = $res->fetch_assoc();

				$credDebiteurNummer = $row['nummer'];
				$credDebiteurNaam = $row['naam'];
			} else {
				nob_assert(isset($omschrijving['NAME']), 'Naam niet kunnen vinden', true);

				$credDebiteurNaam = $omschrijving['NAME'];
			}
		}

		nob_assert($credDebiteurNaam != '', 'Geen naam kunnen vinden', true);

		if (strtotime($maxDate) <= strtotime($transactieDatum)) {
			$qtRekening = ($rekeningnummer == REKENING_NUMMER_LOPEND ? "'lopend'" : "'spaar'");
			$qtOpmerking = 
			$res = nob_exec_query("INSERT IGNORE INTO `gb_bankboek`".
			                     " SET `rekening`={$qtRekening},".
			                         " `valuta`=".nob_quotevalue($valuta).",".
			                         " `datum`=".nob_quotevalue(date('Y-m-d', strtotime($transactieDatum))).",".
			                         " `beginsaldo`=".$fBeginSaldo.",".
			                         " `eindsaldo`=".$fEindSaldo.",".
			                         " `transactiebedrag`=".$fTransactiebedrag.",".
			                         " `beschrijving`=".nob_quotevalue($metaData).",".
			                         " `type`=".nob_quotevalue($credDebiteurType).",".
			                         " `tegenrekening`=".nob_quotevalue($tegenRekening).",".
			                         " `naam`=".nob_quotevalue($credDebiteurNaam).",".
			                         " `opmerking`=".nob_quotevalue($credDebiteurOmschrijving).",".
			                         " `cred/debiteurnummer`=".(int)$credDebiteurNummer);
			nob_assert($res !== false, 'Could not insert grootboek', true);
		}
	}

	$mysqli->commit();
	$mysqli->autocommit(true);

	nob_redirect('bankboek_bijwerken.php');
}

echo "<h1>Grootboek bijwerken</h1>\n";

echo "<p>Het grootboek bijwerken gaat via de ABN site, hier klik je op mutaties downloaden.".
     " Dit gaat voor beide rekeningen in TXT (TAB) formaat.</p>\n";

echo "<form action='".htmlentities($_SERVER['SCRIPT_NAME'])."' method='post' enctype='multipart/form-data'>\n";

echo "<input type='file' name='file'><br>\n";
echo "<input type='submit' name='grootboek_bijwerken'>\n";

echo "</form>\n";

require 'footer.php';
?>