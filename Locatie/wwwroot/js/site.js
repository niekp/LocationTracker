Date.prototype.addDays = function(days) {
    var date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return date;
}
String.prototype.padLeft = function (l, c) { return Array(l - this.length + 1).join(c || " ") + this }

function DateFromString(f_dtmDatum) {
	try {
		f_dtmDatum = f_dtmDatum.toString();
		if (f_dtmDatum.indexOf(' ') > 0) {
			f_dtmDatum = f_dtmDatum.split(' ');
			datum = f_dtmDatum[0];
			tijd = f_dtmDatum[1];
		} else {
			datum = f_dtmDatum;
			tijd = "";
		}

		datum = datum.split('-');
		if (tijd != "")
			tijd = tijd.split(':');

		var dag = datum[0];
		var maand = datum[1];
		var jaar = datum[2];

		if (tijd != "") {
			var uur = tijd[0];
			var minuten = tijd[1];
			var seconden = tijd[2];
		} else{
			var uur = 0;
			var minuten = 0;
			var seconden = 0;
		}

		maand--;

		return new Date(jaar, maand, dag, uur, minuten, seconden);
	}
	catch (err) {
		catchFoutmelding(err);
	}
}

function DateToString(f_dtmDatum) {
	var tijd_datum = new Date(f_dtmDatum);
	var dag = tijd_datum.getDate();
	var maand = tijd_datum.getMonth() + 1;
	var jaar = tijd_datum.getFullYear();

	dag = dag.toString().padLeft(2, "0");
	maand = maand.toString().padLeft(2, "0");
	jaar = jaar.toString().padLeft(4, "0");

	return dag + '-' + maand + '-' + jaar;
}

function randDarkColor() {
	return randomColor({ luminosity: "dark" });
}


$(document).ready(function () {

	$(".datepicker").datepicker({
		format: "dd-mm-yyyy",
		language: "nl"
	});

	$('select').select2({
		theme: 'bootstrap4',
	});

})