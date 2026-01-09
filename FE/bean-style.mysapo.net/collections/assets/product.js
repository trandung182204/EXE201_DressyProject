// assets/js/products.js

document.addEventListener('DOMContentLoaded', function () {
    const API_URL = 'http://localhost:5135/api/products';
    const productsContainer = document.querySelector('.products-view-grid .row');
    if (!productsContainer) return;

    const productTemplate = `
<div class="col-6 col-xl-3 col-lg-4 col-md-4">
    <div class="item_product_main">
        <form action="https://bean-style.mysapo.net/cart/add" method="post" class="variants product-action item-product-main duration-300" data-cart-form data-id="product-actions-{{id}}" enctype="multipart/form-data">
            <a href="javascript:;" class="tag-promo" title="Móc khóa hình gấu đan len">
                <img width="90" height="90" src="//bizweb.dktcdn.net/100/566/174/themes/1008318/assets/icon_fra_1.jpg?1758853455743" alt="Móc khóa hình gấu đan len" />
                <span>Tặng</span>
            </a>
            <div class="product-thumbnail">
                <a class="image_thumb scale_hover" href="../{{slug}}.html" title="{{name}}">
                    <img class="duration-300 image1" src="{{image1}}" alt="{{name}}">
                    <img class="duration-300 image2" src="{{image2}}" alt="{{name}}">
                </a>
                <div class="video_tem">
                    <img width="64" height="64" src="//bizweb.dktcdn.net/100/566/174/themes/1008318/assets/icon_youtube.png?1758853455743" alt="Video"/>
                </div>
                <div class="badge">
                    <span class="new">{{badgeNew}}</span>
                    <span class="best">{{badgeBest}}</span>
                </div>
                <div class="product-button">
                    <input class="hidden" type="hidden" name="variantId" value="{{variantId}}" />
                    <button class="btn-cart btn-views quick-view-option btn btn-primary quick-view" title="Xem nhanh" type="button" data-handle="{{slug}}">Xem nhanh</button>
                    <a href="javascript:void(0)" class="setWishlist btn-views btn-circle" data-wish="{{slug}}" tabindex="0" title="Thêm vào yêu thích">
                        <img width="25" height="25" src="../../bizweb.dktcdn.net/100/566/174/themes/1008318/assets/heartbf6b.png?1758853455743" alt="Thêm vào yêu thích"/> 
                    </a>
                </div>
            </div>
            <div class="product-info">
                <h3 class="product-name line-clamp-1-new">
                    <a href="../{{slug}}.html" title="{{name}}">{{name}}</a>
                </h3>
                <div class="product-price-cart">
                    <span class="price">{{price}}<span class="flash-sale">{{discount}}</span></span>
                    <span class="compare-price">{{comparePrice}}</span>
                </div>
            </div>
        </form>
    </div>
</div>
`;

    function formatVND(amount) {
        if (!amount) return '';
        return Number(amount).toLocaleString('vi-VN') + '₫';
    }

    function getImage(product, idx = 0) {
        if (product.productImages && product.productImages.length > idx) {
            return product.productImages[idx].imageUrl;
        }
        return 'https://via.placeholder.com/300x400?text=No+Image';
    }

    function getVariantId(product) {
        if (product.productVariants && product.productVariants.length > 0) {
            return product.productVariants[0].id;
        }
        return '';
    }

    function getSlug(product) {
        if (product.slug) return product.slug;
        if (product.name) return product.name.toLowerCase().replace(/ /g, '-');
        return '';
    }

    function getComparePrice(product) {
        if (product.comparePrice) return formatVND(product.comparePrice);
        if (product.productVariants && product.productVariants.length > 0) {
            let max = Math.max(...product.productVariants.map(v => v.pricePerDay || 0));
            return max > (product.price || 0) ? formatVND(max) : '';
        }
        return '';
    }

    function getDiscount(product) {
        let price = product.price || 0;
        let compare = product.comparePrice || 0;
        if (!compare && product.productVariants && product.productVariants.length > 0) {
            compare = Math.max(...product.productVariants.map(v => v.pricePerDay || 0));
        }
        if (compare > price && price > 0) {
            return '-' + Math.round(100 * (compare - price) / compare) + '%';
        }
        return '';
    }

    function getBadgeNew(product) {
        if (product.createdAt) {
            const created = new Date(product.createdAt);
            const now = new Date();
            const diff = (now - created) / (1000 * 60 * 60 * 24);
            if (diff < 30) return 'Hàng mới';
        }
        return '';
    }

    function getBadgeBest(product) {
        if (product.totalSold && product.totalSold > 100) return 'Bán chạy';
        return '';
    }

    productsContainer.innerHTML = '';

    fetch(API_URL)
        .then(res => res.json())
        .then(json => {
            const products = json.data || [];
            let html = '';
            products.forEach(product => {
                let card = productTemplate
                    .replace(/{{id}}/g, product.id)
                    .replace(/{{name}}/g, product.name || '')
                    .replace(/{{slug}}/g, getSlug(product))
                    .replace(/{{image1}}/g, getImage(product, 0))
                    .replace(/{{image2}}/g, getImage(product, 1))
                    .replace(/{{variantId}}/g, getVariantId(product))
                    .replace(/{{price}}/g, formatVND(product.price))
                    .replace(/{{comparePrice}}/g, getComparePrice(product))
                    .replace(/{{discount}}/g, getDiscount(product))
                    .replace(/{{badgeNew}}/g, getBadgeNew(product))
                    .replace(/{{badgeBest}}/g, getBadgeBest(product));
                html += card;
            });
            productsContainer.innerHTML = html;
        })
        .catch(() => {
            productsContainer.innerHTML = '<div style="padding:2rem">Không thể tải sản phẩm.</div>';
        });
});
