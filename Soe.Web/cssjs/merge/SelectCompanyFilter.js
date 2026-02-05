function doFilterCompanies() {
    var input = document.getElementById("filterCompanies");
    var filter = input.value.toUpperCase();
    var table = document.getElementsByClassName("tblCompaniesContent")[0];
    var rows = table.getElementsByTagName("tr");

    // Loop through all table rows, and hide those who don't match the search query
    for (var r = 0; r < rows.length; r++) {
        var row = rows[r];
        if (row) {
            var rowVisible = false;
            var cells = row.getElementsByTagName("td");
            if (cells) {
                for (var c = 0; c < cells.length; c++) {
                    var cell = cells[c];
                    if (cell.innerHTML.toUpperCase().indexOf(filter) > -1) {
                        rowVisible = true;
                    }
                }
            }
            if (rowVisible) {
                row.style.display = "";
            } else {
                row.style.display = "none";
            }
        }
    }
}