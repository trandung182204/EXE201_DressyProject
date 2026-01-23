'use strict';

jQuery(function ($) {

  "use strict";

  var window_select = $(window);

  ///////////////////////////// 
  //         count-up       //
  ////////////////////////////
  var number_counter = $('.counter');
  if ($(number_counter).length > 0) {
    $(number_counter).each(function () {
      var $this = $(this),
          countTo = $this.attr('data-count');

      $({ countNum: $this.text() }).animate({
        countNum: countTo
      }, {

        duration: 12000,
        easing: 'linear',
        step: function step() {
          $this.text(Math.floor(this.countNum));
        },
        complete: function complete() {
          $this.text(this.countNum);
          //alert('finished');
        }
      });
    });
  }

  ///////////////////////////// 
  //   sponsors-carousel    //
  ////////////////////////////
  var sp_carousel = $('.dr-main-slider');
  if (sp_carousel.length > 0) {
    sp_carousel.owlCarousel({
      loop: true,
      margin: 10,
      nav: false,
      autoplay: true,
      autoplayTimeout: 4000,
      navText: false,
      items: 1
    });
  }

  ///////////////////////////// 
  //   sponsors-carousel    //
  ////////////////////////////
  var sp_carousel = $('.sponsors-carousel');
  if (sp_carousel.length > 0) {
    sp_carousel.owlCarousel({
      loop: true,
      margin: 10,
      nav: true,
      navText: ["<img src='../assets/dist/img/left.png'/>", "<img src='../assets/dist/img/right.png'/>"],
      responsive: {
        0: {
          items: 1
        },
        600: {
          items: 3
        },
        1100: {
          items: 6
        }
      }
    });
  }

  ///////////////////////////// 
  //     owl-opinion        //
  ////////////////////////////
  var sp_carousel2 = $('.owl-opinion');
  if (sp_carousel2.length > 0) {
    sp_carousel2.owlCarousel({
      loop: true,
      margin: 10,
      nav: true,
      navText: ["<i class='ion-ios-arrow-left'></i>", "<i class='ion-ios-arrow-right'></i>"],
      items: 1
    });
  }

  ///////////////////////////// 
  //    work-procedure      //
  ////////////////////////////
  var sp_carousel3 = $('.owl-favorites');
  if (sp_carousel3.length > 0) {
    sp_carousel3.owlCarousel({
      loop: true,
      margin: 10,
      nav: true,
      navText: ["<img src='../assets/dist/img/left.png'/>", "<img src='../assets/dist/img/right.png'/>"],
      responsive: {
        0: {
          items: 1
        },
        600: {
          items: 3
        },
        1100: {
          items: 5
        }
      }
    });
  }

  ///////////////////////////// 
  // product_details_slider //
  //////////////////////////// 
  var productSlider = $('.product_details_slider');
  if (productSlider.length > 0) {
    productSlider.flexslider({
      animation: "fade",
      slideshow: false,
      controlNav: "thumbnails"
    });
  }

  ///////////////////////////// 
  //      NICE-SCROLL       //
  //////////////////////////// 
  if (window.matchMedia("(min-width: 1200px)").matches) {
    $(".flex-control-thumbs").niceScroll({
      cursorcolor: "#979797",
      cursoropacitymin: 0,
      background: "#D8D8D8",
      cursorborder: "5",
      autohidemode: true,
      cursorminheight: 30
    });

    $(".customer-photo").niceScroll({
      cursorcolor: "#979797",
      cursoropacitymin: 0,
      background: "#D8D8D8",
      cursorborder: "5",
      autohidemode: true,
      cursorminheight: 30
    });
  }

  ///////////////////////////// 
  //      SMOOTH SCROLL      //
  //////////////////////////// 
  var scroll_var = $(".execution .elements .slow-down");
  scroll_var.smoothScroll();

  ///////////////////////////// 
  //        SELECT2         //
  //////////////////////////// 
  var select2_selector = $(".rq-rental-select2");
  select2_selector.select2();

  ///////////////////////////// 
  //       ACCORDION        //
  //////////////////////////// 
  var accordion_header = $(".panel-title a");

  accordion_header.on('click', function () {
    if ($(this).hasClass("collapsed")) {
      $(this).find("i").removeClass("fa-angle-down").addClass("fa-angle-up");
    } else {
      $(this).find("i").removeClass("fa-angle-up").addClass("fa-angle-down");
    }
  });

  ///////////////////////////// 
  //    CUSTOMER CAROUSEL    //
  //////////////////////////// 
  var sp_carousel4 = $('.customer-carousel');
  if (sp_carousel4.length > 0) {
    sp_carousel4.owlCarousel({
      loop: true,
      margin: 10,
      nav: true,
      items: 3,
      navText: ["<i class='ion-ios-arrow-left'></i>", "<i class='ion-ios-arrow-right'></i>"]
    });
  }
  ///////////////////////////// 
  //      TOPBAR SCRIPT      //
  //////////////////////////// 

  var icon = $(".top-bar .ion-ios-close-empty");
  icon.on("click", function () {
    $(".top-bar").addClass("hide-it");
  });

  ///////////////////////////// 
  //      STICKY NAV        //
  //////////////////////////// 

  var sticky = $("#stickynav").offset().top;
  window_select.on('scroll', function () {
    if (window_select.scrollTop() > sticky) {
      $("#stickynav").addClass("navFixed");7;
    } else {
      $("#stickynav").removeClass("navFixed");
    }
  });

  ///////////////////////////// 
  //      RANGE SLIDER       //
  //////////////////////////// 
  var range_selector = $("#range_id");
  range_selector.ionRangeSlider({
    type: "double",
    grid: true,
    min: 0,
    max: 1000,
    from: 200,
    to: 800,
    prefix: "$"
  });

  ///////////////////////////// 
  //         FILTER          //
  //////////////////////////// 
  var filter_selector = $(".filters .dropdown .fa-angle-down");
  filter_selector.on("click", function () {
    $(this).toggleClass("open");
    if ($(this).hasClass("open")) {
      $(this).parents(".dropdown").addClass("rq-collupse");
    } else {
      $(this).parents(".dropdown").removeClass("rq-collupse");
    }
  });

  ///////////////////////////// 
  //  PRODUCT-DISPLAY ICON   //
  //////////////////////////// 
  var iconGift = $("a .fa-gift");
  var iconLike = $("a .fa-heart-o");
  iconGift.on("click", function (e) {
    e.preventDefault();
    $(this).toggleClass("active");
  });
  iconLike.on("click", function (e) {
    e.preventDefault();
    $(this).toggleClass("active");
  });

  var owlCarousel = $('.modal-carousel');
  $('.rq-modal').on('shown.bs.modal', function (event) {
    owlCarousel.owlCarousel({
      loop: true,
      margin: 10,
      nav: true,
      items: 1,
      navText: ["<i class='ion-ios-arrow-left'></i>", "<i class='ion-ios-arrow-right'></i>"]
    });
  });

  ///////////////////////////// 
  //       BIG-SEARCH        //
  //////////////////////////// 
  var click_selector = $(".rq_btn_header_search i");
  var click_selector2 = $(".search-close.close i");
  var event_taker = $(".header-search.open-search");

  click_selector.on("click", function () {
    event_taker.addClass("open");
  });
  click_selector2.on("click", function () {
    event_taker.removeClass("open");
  });

  ///////////////////////////// 
  //       CART ITEM        //
  //////////////////////////// 

  var cart_items = $(".rq-shopping-cart-items-list");
  if (cart_items.length > 0) {
    cart_items.on('click', function (e) {
      e.preventDefault();
      $(this).toggleClass("active");

      if ($(this).hasClass("active")) {
        $(this).parent().find(".rq-shopping-cart-inner-div").addClass("rq-visible");
      } else {
        $(this).parent().find(".rq-shopping-cart-inner-div").removeClass("rq-visible");
      }
    });
    $(document).mouseup(function (e) {
      var container = $(".rq-shopping-cart-inner-div");

      if (!container.is(e.target) // if the target of the click isn't the container...
      && container.has(e.target).length === 0) // ... nor a descendant of the container
        {
          container.removeClass("rq-visible");
          cart_items.removeClass("active");
        }
    });
  }

  ///////////////////////////// 
  //      SCROLL-TO-TOP     //
  //////////////////////////// 
  window_select.on('scroll', function () {
    if ($(this).scrollTop() > 200) {
      $('#go-to-top').fadeIn('slow');
    } else {
      $('#go-to-top').fadeOut('slow');
    }
  });

  $('#go-to-top a').on("click", function () {
    $("html,body").animate({ scrollTop: 0 }, 750);
    return false;
  });

  ///////////////////////////// 
  //      STICKY SIDEBAR     //
  //////////////////////////// 
  window_select.on('scroll', function (e) {
    var $el = $('.fixedElement');
    var isPositionFixed = $el.css('position') == 'fixed';

    if ($(this).scrollTop() > 600 && !isPositionFixed) {
      $('.fixedElement').css({ 'position': 'fixed', 'top': '20px' });
    }
    if ($(this).scrollTop() < 600 && isPositionFixed) {
      $('.fixedElement').css({ 'position': 'static', 'top': '20px' });
    }
  });
});
///////////////////////////// 
//          LOADER         //
////////////////////////////
var window_select = $(window);
window_select.on("load", function () {
  $("#gl-circle-loader-wrapper").fadeOut(500);
});