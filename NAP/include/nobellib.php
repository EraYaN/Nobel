<?php

// joost20141007 Functie welke een query op de geselecteerde database uitvoerd.
// IN:  - query   SQL veilige query welke uitgevoerd wordt door mysqli_query
// UIT: - res     resultaat van mysqli_query of false indien query gefaald is
function nob_exec_query($query) {
	global $mysqli;
	nob_assert(isset($mysqli), 'mysql object is not set', true);

	return $mysqli->query($query);
}

// joost20141016 Zoekt crediteur of debiteurnummer op
// IN:  - id       id voor welke de waarden opgezocht moeten worden
// UIT: - nummers  array koppen met keys 'credit' 'debit' met bijbehorende waarde
function getCreditDebitNummer($id) {
	$result = array();

	$res = nob_exec_query("SELECT `nummer` FROM `crediteur` WHERE `type`='Lid' AND `lid_id`=".(int)$id);
	nob_assert($res !== false, 'Could not look op credit nummer', true);
	nob_assert($res->num_rows == 1, 'Could not find credit nummer', true);
	$row = $res->fetch_assoc();
	$result['credit'] = (int)$row['nummer'];

	$res = nob_exec_query("SELECT `nummer` FROM `debiteur` WHERE `type`='Lid' AND `lid_id`=".(int)$id);
	nob_assert($res !== false, 'Could not look op debit nummer', true);
	nob_assert($res->num_rows == 1, 'Could not find debit nummer', true);
	$row = $res->fetch_assoc();
	$result['debit'] = (int)$row['nummer'];

	return $result;
}

// joost20141018 Functie welke teruggeeft of de bekijken van de pagina paltsgraaf is.
// UIT: - paltsgraaf  boolean of paltsgraaf
function isPaltsgraaf() {
	return ($_SERVER['REMOTE_USER'] == 'joost');
}

// joost20141007 Zorgt dat invoer altijd correct is, geeft anders een foutmelding
// IN:  - code     code welke true moet zijn, bij non-bool ook foutmelding
//      - message  foutmelding welke wordt weergegeven bij foute code
//      - fatal    of bij een foute code het script moet stoppen
// UIT: - code     geeft ingevoerde code terug
function nob_assert($code, $message, $fatal=false) {
	global $mysqli;

	if ($code !== true) {
		foreach (debug_backtrace() as $trace) {
			$message .= "\n".$trace['file'].' ('.$trace['line'].')';
			if ($trace['function'] == 'nob_assert') {
				break;
			}
		}

		if (isset($mysqli) && $mysqli->error != '') {
			$message .= "\n".$mysqli->error;
		}

		if ($fatal) {
			nob_display_message('Fatal error: '.$message, '#F00');
			exit();
		} else {
			nob_display_message($message, '#FF0');
			return $code;
		}
	}
	return $code;
}

// joost20141007 Geeft een duidelijk zichtbare melding
// IN:  - message  bericht wat weergegeven wordt
//      - color    kleur van melding, moet css valid zijn
function nob_display_message($message, $color='#000') {
	$htColor = htmlentities($color);
	echo "<div style='border: 1px dashed $htColor; margin: 2px; padding: 10px;'>\n";
	echo "<p style='font: normal normal 400 16px 1 courier; color: $htColor; margin: 0'>".htmlentities($message).
	     "</p>\n";
	echo "</div>\n";
}

// joost20141015 Print geld op volgende wijze: €10.10
// IN:  - value  numeric value welke omgevormd wordt tot euro waarde
// UIT: - euro   string welke een euro waarde weergeeft
function nob_htEuroValue($value) {
	if ((float)$value === 0.0) {
		return '-';
	}
	return "&euro;".htmlentities(number_format((float)$value, 2, ',', '.'));
}

// joost20141015 redirect je naar een andere pagina
// IN:  - url   url waarnaar geridirect moet worden (slechts deel na)
function nob_redirect($url) {
	if (!headers_sent()) {
		header('Location: '.$_SERVER['SERVER_NAME'].'/'.$url);
	}

	exit();
}

// joost20141007 Maakt van een html onveilige code een html veilige tekst
// IN:  - html  onveilige tekst
// UIT: - html  html xss veilige tekst
function nob_quotevalue($query) {
	global $mysqli;
	nob_assert(isset($mysqli), 'mysql object is not set', true);

	return "'".$mysqli->real_escape_string($query)."'";
}

// joost20141026 Real escape string van een string
// IN:  - string  onveilige string
// UIT  - string  veilige string
function nob_quotestring($string) {
	global $mysqli;
	nob_assert(isset($mysqli), 'mysql object is not set', true);

	if (is_array($string)) {
		$res = array();
		foreach ($string as $key => $value) {
			$res[$key] = nob_quotestring($value);
		}
		return $res;
	}

	return $mysqli->real_escape_string($string);
}
?>