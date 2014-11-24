var isCalculating = false;
var calcValue = '';
var calcCurrent = 0;
var DB_PRECISION = 0.0001;

$(document).ready(function() { 
	$('#form_invoer').submit(function(e) {
		var iTotalPersoonlijk = 0;
		$('.input_persoonlijk').each(function() {
			if ($(this).val()) {
				iTotalPersoonlijk += Math.round($(this).val()/DB_PRECISION);
			}
		});

		if (iTotalPersoonlijk + Math.round($('#ho').val()/DB_PRECISION) != 
		    Math.round($('#totaalbedrag').val()/DB_PRECISION)) {
			alert("HO + totaalPersoonlijke != totaalbedrag\n"+
			      Math.round($('#ho').val()/DB_PRECISION)+" + "+iTotalPersoonlijk+" != "+
			      Math.round($('#totaalbedrag').val()/DB_PRECISION));
			e.preventDefault();
		}
	});

	$('.input_persoonlijk').keypress(function(e) {
		if (isCalculating) {
			if (e.keyCode == 46) { // .
				calcValue += '.';
			} else if (e.keyCode >= 48 && e.keyCode <= 57) { // 0 - 9
				var number = e.keyCode - 48; // value between 0 and 9
				calcValue += String(number);
			} else if (e.keyCode == 13) { // Return
				$(this).val(Math.round((calcCurrent + parseFloat(calcValue))/DB_PRECISION)*DB_PRECISION);
				isCalculating = false;
			} else if (e.keyCode == 43) { // +
				$(this).val(Math.round((calcCurrent + parseFloat(calcValue))/DB_PRECISION)*DB_PRECISION);
				calcCurrent = parseFloat($(this).val());
				calcValue = '';
			}

			e.preventDefault();
		} else if (e.keyCode == 43) { // +
			calcCurrent = parseFloat($(this).val());
			calcValue = '';
			isCalculating = true;

			e.preventDefault();
		}
	});

	$('.input_persoonlijk').focusout(function() {
		isCalculating = false;
	})
});