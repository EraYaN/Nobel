<?
require 'nobellib.php';
require 'header.php';

echo "<h1>Overzicht barbon</h1>\n";

$htLeden = array();
$res = nob_exec_query("SELECT `id`, `voornaam`, `tussenvoegsel`, `achternaam` FROM `lid` WHERE `status`<>'Oud-lid'");
nob_assert($res !== false, 'Kon geen leden selecteren', true);
while (null !== ($row = $res->fetch_assoc())) {
	$htLeden[(int)$row['id']] = htmlentities($row['voornaam'].' '.$row['tussenvoegsel'].' '.$row['achternaam']);
}

$res = nob_exec_query("SELECT `datum`, `gb_barbon`.`nummer`, `bedrag`, `opmerking`, `persoonlijk/ho`, `weging`,".
                            " `crediteur`.`nummer` AS `cred_num`, `crediteur`.`naam` AS `cred_naam`,".
                            " `crediteur`.`lid_id` AS `cred_lid_id`, `debiteur`.`nummer` AS `deb_num`,".
                            " `debiteur`.`naam` `deb_naam`, `debiteur`.`lid_id` AS `deb_lid_id`".
                     " FROM `gb_barbon`".
                     " LEFT JOIN `crediteur`".
                      " ON `crediteur`.`nummer`=`gb_barbon`.`cred/debnummer`".
                     " LEFT JOIN `debiteur`".
                      " ON `debiteur`.`nummer`=`gb_barbon`.`cred/debnummer`".
                     " ORDER BY `gb_barbon`.`datum` DESC, `debiteur`.`lid_id`, `crediteur`.`lid_id`");
nob_assert($res !== false, 'Kon geen barbonnen selecteren', true);

/*
$barbonnen => {
	[<barbonnnummer>] => {
		['timestamp'] => <timestamp>
		['opmerking'] => '<opmerking>'
		['ho'] => <ho>
		['totaalbedrag'] => <totaalbedrag>
		['leden'] => {
			[<lid_id>] => {
				['naam'] =>
				['persoonlijk'] =>
				['weging'] =>
				['deelHO'] =>
			}
		}
		['crediteur']
	}
}
*/
$barbonnen = array();
while (null !== ($row = $res->fetch_assoc())) {
	$nummer = $row['nummer'];
	if (!isset($barbonnen[$nummer])) {
		$barbonnen[$nummer] = array('timestamp'    => (int)strtotime($row['datum']),
		                            'opmerking'    => $row['opmerking'],
		                            'ho'           => 0,
		                            'totaalbedrag' => 0,
		                            'leden'        => array(),
		                            'crediteur'    => -1);
	}

	//Dit is geen lid, dus doet er niet toe (zal wel betaald zijn ofzo #yolo)
	if ($row['cred_lid_id'] == null && $row['deb_lid_id'] == null) {
		$barbonnen[$nummer]['crediteur'] = ($row['cred_num'] != null ? (int)$row['cred_num'] : (int)$row['deb_num']);
		continue;
	}

	//omdat crediteuren nog geld krijgen
	if ($row['cred_lid_id'] != null) {
		$row['bedrag'] *= -1;
	}

	//tel bedragen op voor totaal ho en totaal totaal
	if ($row['persoonlijk/ho'] == 'ho') {
		$barbonnen[$nummer]['ho'] += round($row['bedrag']/DATABASE_CURRENCY_PRECISION);
	}
	$barbonnen[$nummer]['totaalbedrag'] += round($row['bedrag']/DATABASE_CURRENCY_PRECISION);

	$iLidID = ($row['cred_lid_id'] != null ? (int)$row['cred_lid_id'] : (int)$row['deb_lid_id']);
	$lidNaam = ($row['cred_naam'] != null ? $row['cred_naam'] : $row['deb_naam']);
	nob_assert($iLidID != null, 'Lid id kan geen null zijn', true);
	nob_assert($lidNaam != null, 'Lid naam kan geen null zijn', true);

	if (!isset($barbonnen[$nummer]['leden'][$iLidID])) {
		$barbonnen[$nummer]['leden'][$iLidID] = array('naam'        => $lidNaam,
		                                               'persoonlijk' => 0,
		                                               'weging'      => 0,
		                                               'deelHO'      => 0);
	}

	if ($row['persoonlijk/ho'] == 'persoonlijk') {
		$barbonnen[$nummer]['leden'][$iLidID]['persoonlijk'] = (float)$row['bedrag'];
	}
	if ($row['persoonlijk/ho'] == 'ho') {
		$barbonnen[$nummer]['leden'][$iLidID]['deelHO'] = (float)$row['bedrag'];
		nob_assert($row['weging'] != null, 'PANIC, database inconsistentie!!! weging is null bij ho', true);
		$barbonnen[$nummer]['leden'][$iLidID]['weging'] = (int)$row['weging'];
	}
}

foreach ($barbonnen as $barbonnnummer => $barbon) {
	$htNummer = isPaltsgraaf() ? "<span><a href='bewerk_barbon.php?nummer=".htmlentities($barbonnnummer)."'>".
	                             htmlentities($barbonnnummer)."</a></span>" : htmlentities($barbonnnummer);
	echo "<h2>{$htNummer} (".date('l d F Y', $barbon['timestamp']).")</h2>\n";
	echo "<p>".htmlentities($barbon['opmerking'])."</p>\n";
	if ($barbon['crediteur'] != -1) {
		$qtCredDeb = (floor($barbon['crediteur'] / 10000) == 1 ? 'cred' : 'deb');
		$res = nob_exec_query("SELECT `naam`".
		                     " FROM `{$qtCredDeb}iteur`".
		                     " WHERE `nummer`=".(int)$barbon['crediteur']);
		nob_assert($res !== false, 'Kon geen cred/debiteuren selecteren', true);
		$row = $res->fetch_assoc();
		echo "<p>".htmlentities($row['naam'])."</p>\n";
	}

	echo "<table class='data'>\n";

	echo "<tr>\n";
	echo "<td>Naam</td>\n";
	echo "<td>Persoonlijk</td>\n";
	echo "<td>Weging</td>\n";
	echo "<td>Deel HO</td>\n";
	echo "<td>Totaal</td>\n";
	echo "</tr>\n";

	foreach ($barbon['leden'] as $lidID => $lid) {
		echo "<tr>\n";
		echo "<td>".htmlentities($lid['naam'])."</td>\n";
		echo "<td>".nob_htEuroValue($lid['persoonlijk'])."</td>\n";
		echo "<td>".(int)$lid['weging']."</td>\n";
		echo "<td>".nob_htEuroValue($lid['deelHO'])."</td>\n";
		echo "<td>".nob_htEuroValue($lid['deelHO'] + $lid['persoonlijk'])."</td>\n";
		echo "</tr>\n";
	}

	echo "<tr class='table_sum'>\n";
	echo "<td>&nbsp;</td>\n";
	echo "<td>&nbsp;</td>\n";
	echo "<td>&nbsp;</td>\n";
	echo "<td>".nob_htEuroValue(round($barbon['ho']*DATABASE_CURRENCY_PRECISION, 4))."</td>\n";
	echo "<td>".nob_htEuroValue(round($barbon['totaalbedrag']*DATABASE_CURRENCY_PRECISION, 4))."</td>\n";
	echo "</tr>\n";

	echo "</table>\n";

	echo "<hr>\n";
}

require 'footer.php';
?>