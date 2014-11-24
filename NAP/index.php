<?
require 'nobellib.php';
require 'header.php';

echo "<h1>Saldo</h1>\n";

$res = nob_exec_query("SELECT `id`, `voornaam`, `tussenvoegsel`, `achternaam`, `ideele_stand`".
                     " FROM `lid`".
                     " WHERE `status`<>'Oud-lid'");
nob_assert($res !== false, 'Could not select leden', true);

echo "<table class='data'>\n";

echo "<tr>\n";
echo "<td>Naam</td>\n";
echo "<td>Huidig saldo</td>\n";
echo "<td>Ideele stand</td>\n";
echo "<td>Over te maken</td>\n";
echo "</tr>\n";

$iTotalHuidigSaldo = 0;
$iTotalIdeeleStand = 0;
$iTotalOverTeMaken = 0;

while (null !== ($row = $res->fetch_assoc())) {
	$credDebitNummer = getCreditDebitNummer($row['id']);

	$res2 = nob_exec_query("SELECT SUM(`bedrag`)".
	                      " FROM `gb_crediteur`".
	                      " WHERE `crediteurnummer`=".(int)$credDebitNummer['credit']);
	nob_assert($res2 !== false, 'Kon geen bedrag uit gb_crediteur halen', true);
	list($sumCredit) = $res2->fetch_row();
	$iSumCredit = round($sumCredit/DATABASE_CURRENCY_PRECISION);

	$res2 = nob_exec_query("SELECT SUM(`bedrag`)".
	                      " FROM `gb_debiteur`".
	                      " WHERE `debiteurnummer`=".(int)$credDebitNummer['debit']);
	nob_assert($res2 !== false, 'Kon geen bedrag uit gb_debiteur halen', true);
	list($sumDebit) = $res2->fetch_row();
	$iSumDebit = round($sumDebit/DATABASE_CURRENCY_PRECISION);

	$res2 = nob_exec_query("SELECT SUM(`transactiebedrag`)".
	                      " FROM `gb_bankboek`".
	                      " WHERE `cred/debiteurnummer`=".(int)$credDebitNummer['credit'].
	                      " OR `cred/debiteurnummer`=".(int)$credDebitNummer['debit']);
	nob_assert($res2 !== false, 'Kon geen bedrag uit gb_debiteur halen', true);
	list($sumBankboek) = $res2->fetch_row();
	$iSumBankboek = round($sumBankboek/DATABASE_CURRENCY_PRECISION);

	$iHuidigSaldo = $iSumCredit - $iSumDebit + $iSumBankboek;
	$iOverTeMaken = round($row['ideele_stand']/DATABASE_CURRENCY_PRECISION) - $iHuidigSaldo;
	$htOverTeMaken = ($iOverTeMaken > 0 ? nob_htEuroValue(round($iOverTeMaken*DATABASE_CURRENCY_PRECISION, 4)) : '-');

	$iTotalHuidigSaldo += $iHuidigSaldo;
	$iTotalIdeeleStand += round($row['ideele_stand']/DATABASE_CURRENCY_PRECISION);
	$iTotalOverTeMaken += max(0, $iOverTeMaken);

	echo "<tr>\n";
	echo "<td>".htmlentities($row['voornaam'].' '.$row['tussenvoegsel'].' '.$row['achternaam'])."</td>\n";
	echo "<td>".nob_htEuroValue(round($iHuidigSaldo*DATABASE_CURRENCY_PRECISION, 4))."</td>\n";
	echo "<td>".nob_htEuroValue(round($row['ideele_stand'], 4))."</td>\n";
	echo "<td>{$htOverTeMaken}</td>\n";
	echo "</tr>\n";
}

if (isPaltsgraaf()) {
	echo "<tr class='table_sum'>\n";
	echo "<td>&nbsp;</td>\n";
	echo "<td>".nob_htEuroValue(round($iTotalHuidigSaldo*DATABASE_CURRENCY_PRECISION, 4))."</td>\n";
	echo "<td>".nob_htEuroValue(round($iTotalIdeeleStand*DATABASE_CURRENCY_PRECISION, 4))."</td>\n";
	echo "<td>".nob_htEuroValue(round($iTotalOverTeMaken*DATABASE_CURRENCY_PRECISION, 4))."</td>\n";
	echo "</tr>\n";
}

echo "</table>\n";

require 'footer.php';
?>