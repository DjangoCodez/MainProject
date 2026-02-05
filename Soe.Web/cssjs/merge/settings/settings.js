function showHideClass (className, show) 
{
    var all = document.all ? document.all : document.getElementsByTagName('*');    
    for (var i = 0; i < all.length; i++)
    {
        var element = all[i];
        if (element.className == className)
        {
            if (null == show) 
                show = element.style.display == 'none';
            element.style.display = (show ? '' : 'none');
        }
    }
}

function showHideClasses(className1, className2, show)
{
    showHideClass(className1, show);
    showHideClass(className2, show);
}
