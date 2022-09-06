import { useState, useEffect } from "react";
import { Product } from "../../models/product"
import ProductList from "./ProductList";

export default function Catalog() {

    const [products, setProducts] = useState<Product[]>([]);


    useEffect(() => {
        //agent.Catalog.list().then(products => setProducts(products))
        fetch('http://localhost:5000/api/Products')
            .then(response => response.json())
            .then(data => setProducts(data))
    }, [])

    return (
        <>
            <ProductList products={products} />
        </>
    )
}