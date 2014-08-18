var myLocalStorage = (function() {
	function getSetBool(key, value, defaultValue) {
		if (typeof value !== 'undefined') {
			localStorage.setItem(key, value);
			return value;
		} else {
			if (typeof localStorage[key] === "string") {
				 return localStorage[key] === "true";
			} else {
				return defaultValue;
			}
		}
	}

	return {
		showInherited : function(value) {
			return getSetBool('showInherited', value, true);
		},

		showProtected : function(value) {
			return getSetBool('showProtected', value, true);
		}
	};

})();

$(function() {
	function bindEvents() {
		// handler for expand/collapse tree
		$('.js-expander').click(function() {
			var $jsExpander = $(this),
				$li = $jsExpander.closest('li'),
				$ol = $li.children('ol');

			if ($jsExpander.hasClass('js-collapsed')) {
				$jsExpander.removeClass('js-collapsed');
				$jsExpander.text('-');
				$ol.slideDown();
			} else {
				$jsExpander.addClass('js-collapsed');
				$jsExpander.text('+');
				$ol.slideUp();
			}
		});

		$('.js-show-inherited').change(function() {
			var showInherited = $(this).prop("checked");

			// propagate to the same checkboxes
			$('.js-show-inherited').prop("checked", showInherited);

			// save to local storage
			myLocalStorage.showInherited(showInherited);

			if (showInherited) {
				// show inherited
				$("tr.inherited").show();

				// but not protected, if not set
				if (!myLocalStorage.showProtected()) {
					$("tr.Family").hide();
				}
			} else {
				// hide inherited (perhaps also protected)
				$("tr.inherited").hide();
			}
		});

		$('.js-show-protected').change(function() {
			var showProtected = $(this).prop("checked");

			// propagate to the same checkboxes
			$('.js-show-protected').prop("checked", showProtected);

			// save to local storage
			myLocalStorage.showProtected(showProtected);

			if (showProtected) {
				// show protected
				$("tr.Family").show();

				// but not protected, if not set
				if (!myLocalStorage.showInherited()) {
					$("tr.inherited").hide();
				}
			} else {
				// hide protected (perhaps also inherited)
				$("tr.Family").hide();
			}
		});
	}


	bindEvents();

	// collapse up to class level initially
	$('nav > ol > li > ol > li .js-expander').each(function() {
		var $jsExpander = $(this),
			$li = $jsExpander.closest('li'),
			$ol = $li.children('ol');

		$jsExpander.addClass('js-collapsed');
		$jsExpander.text('+');
		$ol.hide();
	});

	// but expand the selected and mark it bold
	$('nav a').each(function() {
		var $a = $(this),
			$li = $a.closest('li'),
			$parents;

		if (window.location.href.indexOf($a.attr('href')) > 0) {
			$li.addClass('selected');
			$parents = $a.parents('li');
			$parents.each(function() {
				var $parent = $(this);
				var $jsExpander = $parent.children('.js-expander');
				var $ol = $parent.children('ol');
				$jsExpander.removeClass('js-collapsed');
				$jsExpander.text('-');
				$ol.show();
			});
		}
	});

	$('.js-show-inherited').prop('checked', myLocalStorage.showInherited());
	$('.js-show-protected').prop('checked', myLocalStorage.showProtected());

	if (!myLocalStorage.showInherited()) {
		$("tr.inherited").hide();
	}

	if (!myLocalStorage.showProtected()) {
		$("tr.protected").hide();
	}
});
