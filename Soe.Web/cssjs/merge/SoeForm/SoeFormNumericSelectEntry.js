function SynchNumeric(numericID, selectID)
{
    var numeric = $$(numericID);
    var select = $$(selectID);

    if (numeric == null || select == null)
    {
        return;
    }

    if (select.value != null && select.value != '' && select.value > 0)
        numeric.value = select.value;
    else
        numeric.value = '';
}

function SynchSelect(numericID, selectID)
{
    var numeric = $$(numericID);
    var select = $$(selectID);

    if (numeric == null || select == null)
    {
        return;
    }

    if (numeric.value != null || numeric.value != '')
        select.value = numeric.value;
    else
        select.value = 0;
}
