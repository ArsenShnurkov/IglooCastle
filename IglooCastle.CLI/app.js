var myLocalStorage = {
	hideInherited : function(value) {
		if (typeof value !== 'undefined') {
			if (value) {
				localStorage.setItem("hide-inherited", !!value);
			} else {
				localStorage.removeItem("hide-inherited");
			}

			return value;
		} else {
			return localStorage['hide-inherited'];
		}
	}
};

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
			var isChecked = $(this).prop("checked");
			$('.js-show-inherited').prop("checked", isChecked);
			if (isChecked) {
				$("tr.inherited").show();
			} else {
				$("tr.inherited").hide();
			}

			myLocalStorage.hideInherited(!isChecked);
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

	if (myLocalStorage.hideInherited()) {
		$('tr.inherited').hide();
		$('.js-show-inherited').prop('checked', false);
	}
});
