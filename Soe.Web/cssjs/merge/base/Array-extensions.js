Array.prototype.contains = function (element) {
	for (var i = 0; i < this.length; i++) {
		if (this[i] == element)
			return true;
	}
	return false;
};

Array.prototype.remove = function(element) {
	var i = this.indexOf(element);
	if (i != -1)
		this.splice(i, 1);
	console.log(this);
}

Array.prototype.each = function (fn) {
	if (fn) {
		for (var i = 0, il = this.length; i < il; i++) {
			fn.call(this[i]);
		}
	}
	return this;
};
