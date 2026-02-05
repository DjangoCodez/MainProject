//Has time suffix to avoid name collisions when this UserControl is placed inside a page
var time_intervalId = -1;
var time_sysTerms = null;
var time_validations = 0;

function time_init()
{
    time_initTerms();
}

function time_initTerms()
{
    //create
    time_sysTerms = new Array();
    time_sysTerms.push(TermManager.createSysTerm(1, 5551, 'Du måste ange en giltig tid'));

    //validate
    time_intervalId = setInterval(time_validateTerms, TermManager.delay);
}

function time_validateTerms()
{
    time_validations++;
    var valid = TermManager.validateSysTerms(time_sysTerms, 'time');
    if (valid || TermManager.reachedMaxValidations(time_validations))
    {
        clearInterval(time_intervalId);
        time_setup();
    }
}

function time_setup()
{
    //Add initialization here!
}

function formatTimeEmptyIfPossible(field, useRealTime) {
    try {
        var temp = field.value;

        var boundary = 5;
        if (useRealTime)
            boundary = 4;

        //Empty
        if (temp.length <= 0) {
            field.value = '';
            field.focus();
            return 0;
        }

        //Remove negative
        var isNegative = false;
        if (Left(temp, 1) == '-') {
            isNegative = true;
            temp = Right(temp, temp.length - 1);
        }

        //Replace space, point and comma
        temp = temp.replace(' ', '').replace('.', ':').replace(',', ':');

        //Ensure ends with 0
        if (temp.lastIndexOf(':') == temp.length - 2 && temp.length > 1)
            temp += "0";

        //Replace colon
        temp = temp.replace(':', '');
        
        //Max length 4 or 5 (depending on useRealTime parameter)
        if (temp.length > boundary)
            temp = left(temp, boundary);

        //Invalid - not a number
        if (isNaN(temp)) {
            alert(TermManager.getText(time_sysTerms, 1, 5551));
            field.value = '0:00';
            field.focus();
            return 0;
        }

        //Invalid - more than 59 min
        if ((temp.length > 2) && (parseInt(Right(temp, 2)) > 59)) {
            alert(TermManager.getText(time_sysTerms, 1, 5551));
            field.value = '0:00';
            field.focus();
            return 0;
        }

        //Format
        if (temp.length == 5)
            temp = Left(temp, 3) + ':' + Right(temp, 2);
        else if (temp.length == 4)
            temp = Left(temp, 2) + ':' + Right(temp, 2);
        else if (temp.length == 3)
            temp = Left(temp, 1) + ':' + Right(temp, 2);
        else if (temp.length < 3)
            temp = temp + ':00';

        //Add negative
        if (isNegative == true)
            temp = '-' + temp;

        field.value = temp;
    }
    catch (err) {
        field.value = '';
    }
}

function formatTime(field)
{
    try
    {
        var temp = field.value;

        //Empty
        if (temp.length <= 0)
        {
            field.value = '0:00';
            field.focus();
            return 0;
        }

        //Replace space, point and comma
        temp = temp.replace(' ', '').replace('.', ':').replace(',', ':')

        //Ensure ends with 0
        if(temp.lastIndexOf(':')==temp.length-2&&temp.length>1)
            temp += "0";

        //Replace colon
        temp = temp.replace(':', '');

        //Max length 4
        if (temp.length > 4)
            temp = left(temp, 4);

        //Invalid - not a number
        if (isNaN(temp))
        {
            alert(TermManager.getText(time_sysTerms, 1, 5551));
            field.value = '0:00';
            field.focus();
            return 0;
        }

        //Invalid - more than 59 min
        if ((temp.length > 2) && (parseInt(Right(temp, 2)) > 59))
        {
            alert(TermManager.getText(time_sysTerms, 1, 5551));
            field.value = '0:00';
            field.focus();
            return 0;
        }

        //Format
        if (temp.length == 5)
            field.value = Left(temp, 3) + ':' + Right(temp, 2)
        if (temp.length == 4)
            field.value = Left(temp, 2) + ':' + Right(temp, 2)
        if (temp.length == 3)
            field.value = Left(temp, 1) + ':' + Right(temp, 2)
        if (temp.length < 3)
            field.value = temp + ':00';
    }
    catch (err)
    {
        field.value='0:00';
    }
}

$(window).bind('load', time_init);