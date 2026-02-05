// addEvent and removeEvent, designed by Aaron Moore
function addExtendedEvent(element, listener, handler)
{
	//if the system is not set up, set it up, and
	// store any outside script's event registration in the first handler slot
	if(typeof element[listener] != 'function' || 
	typeof element[listener + '_num'] == 'undefined'){
		element[listener + '_num'] = 0;
		if(typeof element[listener] == 'function'){
			element[listener + 0] = element[listener];
			element[listener + '_num']++;
		}
		element[listener] = function(e){
			var r = true;
			e = (e) ? e : window.event;
			for(var i = 0; i < element[listener + '_num']; i++)
				if(element[listener + i](e) === false) r = false;
			return r;
		}
	}
	//if handler is not already stored, assign it
	for(var i = 0; i < element[listener + '_num']; i++)
		if(element[listener + i] == handler) return;
	element[listener + element[listener + '_num']] = handler;
	element[listener + '_num']++;
}
function removeExtendedEvent(element, listener, handler)
{
	//if the system is not set up, or there are no handlers to remove, exit
	if(typeof element[listener] != 'function' || 
	typeof element[listener + '_num'] == 'undefined' ||
	element[listener + '_num'] == 0) return;
	//loop through handlers,
	//  if target handler is reached, begin overwriting each
	//  handler with the handler in front of it until one before the last
	var found = false;
	for(var i = 0; i < element[listener + '_num']; i++){
		if(!found)
			found = element[listener + i] == handler;
		if(found && (i+1) < element[listener + '_num'])
			element[listener + i] = element[listener + (i+1)];
	}
	//if handler was found, decrement the handler count
	if(found)
		element[listener + '_num']--;
}