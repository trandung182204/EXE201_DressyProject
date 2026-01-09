// assets/js/products.js
document.addEventListener('DOMContentLoaded', function () {
    const productsContainer = document.querySelector('.products-view-grid .row');
    if (!productsContainer) return;

    // Clear existing products
    productsContainer.innerHTML = '';

    // Test product với link ảnh fix cứng
    const testProduct = {
        id: 44617579,
        name: 'Áo khoác blazer',
        slug: 'ao-khoac-blazer',
        price: 705000,
        comparePrice: 850000,
        vendor: 'Blazer',
        image1: 'https://bizweb.dktcdn.net/thumb/large/100/566/174/products/ao-khoac-blazer-co-v-alice1.jpg?v=1744967616113',
        image2: 'https://bizweb.dktcdn.net/thumb/large/100/566/174/products/ao-khoac-blazer-co-v-alice2.jpg?v=1744967616113',
        variantId: 144164321
    };

    const productHTML = `
<div class="col-6 col-xl-3 col-lg-4 col-md-4">
    <div class="item_product_main">
        <form action="https://bean-style.mysapo.net/cart/add" method="post" class="variants product-action item-product-main duration-300" enctype="multipart/form-data">
            <div class="product-thumbnail">
                <a class="image_thumb scale_hover" href="../${testProduct.slug}.html" title="${testProduct.name}">
                    <img src="${testProduct.image1}" alt="${testProduct.name}" width="300" height="400">
                    <img src="${testProduct.image2}" alt="${testProduct.name}" width="300" height="400">
                </a>
            </div>
            <div class="product-info">
                <span class="vendor_name">${testProduct.vendor}</span>
                <h3 class="product-name">
                    <a href="../${testProduct.slug}.html" title="${testProduct.name}">${testProduct.name}</a>
                </h3>
                <div class="product-price-cart">
                    <span class="price">${testProduct.price.toLocaleString('vi-VN')}₫</span>
                    <span class="compare-price">${testProduct.comparePrice.toLocaleString('vi-VN')}₫</span>
                </div>
                <input type="hidden" name="variantId" value="${testProduct.variantId}" />
                <button type="submit" class="btn btn-primary">Thêm vào giỏ</button>
            </div>
        </form>
    </div>
</div>
`;

    productsContainer.innerHTML = productHTML;
});
