"use client";

import { useEffect, useMemo, useState } from "react";
import {
  Button,
  Card,
  Form,
  Input,
  InputNumber,
  Tabs,
  Tag,
  message as antMessage,
} from "antd";
import { OrderTable, Product, ProductTable, Order } from "./ShopTables";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5115/api";

export type Role = "Admin" | "Shopkeeper" | "Customer";

type Shop = {
  id: string;
  shopName: string;
  address: string;
  category: string;
  isOpen: boolean;
};

type CartItem = Product & { quantity: number };

async function request(
  path: string,
  options: RequestInit = {},
  token?: string,
) {
  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options.headers ?? {}),
    },
  });
  const data = await response.json().catch(() => ({}));
  if (!response.ok)
    throw new Error(data.message ?? "Something went wrong. Please try again.");
  return data;
}

const formatMoney = (amount: number) =>
  new Intl.NumberFormat("en-IN", { style: "currency", currency: "INR" }).format(
    amount,
  );

export default function LocalShopManagement() {
  const [role, setRole] = useState<Role>("Customer");
  const [session, setSession] = useState<{ token: string; role: Role } | null>(
    null,
  );
  const [mode, setMode] = useState<"login" | "register">("login");
  const [form] = Form.useForm();

  useEffect(() => {
    const saved = localStorage.getItem("local-shop-session");
    if (saved) setSession(JSON.parse(saved));
  }, []);

  const signOut = () => {
    localStorage.removeItem("local-shop-session");
    setSession(null);
  };

  const signIn = async (values: Record<string, string>) => {
    try {
      if (mode === "register") {
        await request("/Auth/register", {
          method: "POST",
          body: JSON.stringify(values),
        });
        setMode("login");
        antMessage.success("Account created. Sign in to start shopping.");
        form.resetFields();
        return;
      }
      const data = await request("/Auth/login", {
        method: "POST",
        body: JSON.stringify({
          email: values.email,
          password: values.password,
        }),
      });
      if (data.role !== role)
        throw new Error(
          `This account is a ${data.role}. Choose that sign-in type to continue.`,
        );
      const next = { token: data.token, role: data.role as Role };
      localStorage.setItem("local-shop-session", JSON.stringify(next));
      window.location.assign(`/${next.role.toLowerCase()}`);
    } catch (error) {
      antMessage.error(
        error instanceof Error ? error.message : "Unable to continue.",
      );
    }
  };

  if (session) return <Dashboard session={session} onSignOut={signOut} />;
  return (
    <main className="landing">
      <section className="hero">
        <span className="eyebrow">LOCAL • SIMPLE • FAST</span>
        <h1>Your local shops, all in one place.</h1>
        <p>
          Order from nearby shops or run your business from a single, clear
          dashboard.
        </p>
        <div className="role-cards">
          {(["Customer", "Shopkeeper", "Admin"] as Role[]).map((item) => (
            <Card
              key={item}
              hoverable
              className={role === item ? "role-card selected" : "role-card"}
              onClick={() => {
                setRole(item);
                setMode(item === "Customer" ? mode : "login");
                form.resetFields();
              }}
            >
              <strong>
                {item === "Customer"
                  ? "Shop & order"
                  : item === "Shopkeeper"
                    ? "Manage your shop"
                    : "Manage the network"}
              </strong>
              <p>
                {item === "Customer"
                  ? "Browse local products and track orders"
                  : item === "Shopkeeper"
                    ? "Keep products, stock and orders organised"
                    : "Create and support shopkeeper accounts"}
              </p>
            </Card>
          ))}
        </div>
      </section>
      <Card className="auth-card">
        <span className="eyebrow">{role} PORTAL</span>
        <h2>
          {mode === "register" ? "Create your customer account" : "Welcome back"}
        </h2>
        <p>
          {mode === "register"
            ? "Create an account to place local orders."
            : `Sign in as a ${role.toLowerCase()}.`}
        </p>
        <Form form={form} layout="vertical" onFinish={signIn}>
          {mode === "register" && (
            <>
              <Form.Item name="name" label="Name" rules={[{ required: true }]}>
                <Input placeholder="Your full name" />
              </Form.Item>
              <Form.Item name="phone" label="Phone" rules={[{ required: true }]}>
                <Input placeholder="Your phone number" />
              </Form.Item>
            </>
          )}
          <Form.Item
            name="email"
            label="Email"
            rules={[{ required: true, type: "email" }]}
          >
            <Input placeholder="you@example.com" />
          </Form.Item>
          <Form.Item
            name="password"
            label="Password"
            rules={[{ required: true, min: 6 }]}
          >
            <Input.Password placeholder="••••••••" />
          </Form.Item>
          <Button type="primary" htmlType="submit" block>
            {mode === "register" ? "Create account" : "Sign in"}
          </Button>
        </Form>
        {role === "Customer" && (
          <Button
            type="link"
            onClick={() => {
              setMode(mode === "login" ? "register" : "login");
              form.resetFields();
            }}
          >
            {mode === "login"
              ? "New here? Create a customer account"
              : "Already registered? Sign in"}
          </Button>
        )}
        <small>Shopkeepers are created by an administrator.</small>
      </Card>
    </main>
  );
}

function Dashboard({
  session,
  onSignOut,
}: {
  session: { token: string; role: Role };
  onSignOut: () => void;
}) {
  return (
    <main className="app-shell">
      <header>
        <a className="brand">Neighbourhood Hub</a>
        <Tag color="blue">{session.role}</Tag>
        <Button type="text" onClick={onSignOut}>
          Sign out
        </Button>
      </header>
      {session.role === "Admin" ? (
        <AdminPanel token={session.token} />
      ) : session.role === "Shopkeeper" ? (
        <ShopkeeperPanel token={session.token} />
      ) : (
        <CustomerPanel token={session.token} />
      )}
    </main>
  );
}

export function AdminPanel({ token }: { token: string }) {
  const [form] = Form.useForm();
  const submit = async (values: Record<string, string>) => {
    try {
      const data = await request(
        "/Admin/create-shopkeeper",
        { method: "POST", body: JSON.stringify(values) },
        token,
      );
      antMessage.success(data.message);
      form.resetFields();
    } catch (error) {
      antMessage.error(
        error instanceof Error ? error.message : "Unable to create shopkeeper.",
      );
    }
  };
  return (
    <section className="dashboard">
      <div className="page-intro">
        <span className="eyebrow">ADMIN CONSOLE</span>
        <h1>Grow your shop network.</h1>
        <p>
          Give verified shop owners their access. They can then create their own
          shop, products and fulfil orders.
        </p>
      </div>
      <div className="two-column">
        <Card title="Create shopkeeper">
          <Form form={form} layout="vertical" onFinish={submit}>
            <Form.Item name="name" label="Full name" rules={[{ required: true }]}>
              <Input />
            </Form.Item>
            <Form.Item
              name="email"
              label="Email"
              rules={[{ required: true, type: "email" }]}
            >
              <Input />
            </Form.Item>
            <Form.Item name="phone" label="Phone" rules={[{ required: true }]}>
              <Input />
            </Form.Item>
            <Form.Item
              name="password"
              label="Temporary password"
              rules={[{ required: true, min: 6 }]}
            >
              <Input.Password />
            </Form.Item>
            <Button type="primary" htmlType="submit">
              Create shopkeeper
            </Button>
          </Form>
        </Card>
        <Card title="Access by role" className="info-card">
          <p>
            <strong>Admin</strong> creates shopkeeper accounts.
          </p>
          <p>
            <strong>Shopkeeper</strong> creates one shop, maintains inventory
            and updates orders.
          </p>
          <p>
            <strong>Customer</strong> browses open shops, adds products to a
            cart and tracks personal orders.
          </p>
        </Card>
      </div>
    </section>
  );
}

export function ShopkeeperPanel({
  token,
  initialTab = "inventory",
}: {
  token: string;
  initialTab?: "inventory" | "orders" | "shop";
}) {
  const [products, setProducts] = useState<Product[]>([]);
  const [orders, setOrders] = useState<Order[]>([]);
  const [tab, setTab] = useState<"inventory" | "orders" | "shop">(initialTab);
  const [shopForm] = Form.useForm();
  const [productForm] = Form.useForm();

  const load = async () => {
    try {
      const [productData, orderData] = await Promise.all([
        request("/Product/my-products", {}, token),
        request("/Order/shop-orders", {}, token),
      ]);
      setProducts(productData.products ?? []);
      setOrders(orderData.orders ?? []);
    } catch {
      /* A new shop has no data yet. */
    }
  };
  useEffect(() => {
    load();
  }, []);

  const createShop = async (values: Record<string, string | number>) => {
    try {
      const data = await request(
        "/Shop",
        {
          method: "POST",
          body: JSON.stringify({
            ...values,
            latitude: Number(values.latitude || 0),
            longitude: Number(values.longitude || 0),
          }),
        },
        token,
      );
      antMessage.success(data.message);
      shopForm.resetFields();
    } catch (e) {
      antMessage.error(e instanceof Error ? e.message : "Unable to create shop.");
    }
  };

  const createProduct = async (values: Record<string, string | number>) => {
    try {
      const data = await request(
        "/Product",
        {
          method: "POST",
          body: JSON.stringify({
            ...values,
            price: Number(values.price),
            stock: Number(values.stock),
          }),
        },
        token,
      );
      antMessage.success(data.message);
      productForm.resetFields();
      load();
    } catch (e) {
      antMessage.error(e instanceof Error ? e.message : "Unable to add product.");
    }
  };

  const updateStatus = async (id: string, status: string) => {
    try {
      await request(
        `/Order/${id}/status`,
        { method: "PUT", body: JSON.stringify({ status }) },
        token,
      );
      load();
    } catch (e) {
      antMessage.error(
        e instanceof Error ? e.message : "Unable to update order.",
      );
    }
  };

  return (
    <section className="dashboard">
      <div className="page-intro">
        <span className="eyebrow">SHOPKEEPER DASHBOARD</span>
        <h1>Run today's shop, calmly.</h1>
        <p>
          Add your shop once, keep stock current, and move customer orders
          through the queue.
        </p>
      </div>
      <Tabs
        activeKey={tab}
        onChange={(key) => setTab(key as typeof tab)}
        items={[
          { key: "inventory", label: "Inventory" },
          { key: "orders", label: "Orders" },
          { key: "shop", label: "Shop" },
        ]}
      />
      {tab === "shop" && (
        <Card title="Create your shop" className="form-panel">
          <Form form={shopForm} layout="vertical" onFinish={createShop}>
            <Form.Item
              name="shopName"
              label="Shop name"
              rules={[{ required: true }]}
            >
              <Input />
            </Form.Item>
            <Form.Item
              name="address"
              label="Address"
              rules={[{ required: true }]}
            >
              <Input />
            </Form.Item>
            <Form.Item
              name="category"
              label="Category"
              rules={[{ required: true }]}
            >
              <Input placeholder="Groceries, Bakery…" />
            </Form.Item>
            <div className="form-grid">
              <Form.Item name="latitude" label="Latitude (optional)">
                <InputNumber style={{ width: "100%" }} step={0.0001} />
              </Form.Item>
              <Form.Item name="longitude" label="Longitude (optional)">
                <InputNumber style={{ width: "100%" }} step={0.0001} />
              </Form.Item>
            </div>
            <Button type="primary" htmlType="submit">
              Save shop
            </Button>
          </Form>
        </Card>
      )}
      {tab === "inventory" && (
        <>
          <Card title="Add product" className="form-panel">
            <Form form={productForm} layout="vertical" onFinish={createProduct}>
              <div className="form-grid">
                <Form.Item name="name" label="Name" rules={[{ required: true }]}>
                  <Input />
                </Form.Item>
                <Form.Item
                  name="category"
                  label="Category"
                  rules={[{ required: true }]}
                >
                  <Input />
                </Form.Item>
                <Form.Item
                  name="price"
                  label="Price"
                  rules={[{ required: true }]}
                >
                  <InputNumber style={{ width: "100%" }} min={0} step={0.01} />
                </Form.Item>
                <Form.Item
                  name="stock"
                  label="Stock"
                  rules={[{ required: true }]}
                >
                  <InputNumber style={{ width: "100%" }} min={0} />
                </Form.Item>
              </div>
              <Form.Item
                name="description"
                label="Description"
                rules={[{ required: true }]}
              >
                <Input.TextArea />
              </Form.Item>
              <Button type="primary" htmlType="submit">
                Add product
              </Button>
            </Form>
          </Card>
          <ProductTable products={products} />
        </>
      )}
      {tab === "orders" && (
        <OrderTable orders={orders} allowUpdate onUpdate={updateStatus} />
      )}
    </section>
  );
}

export function CustomerPanel({
  token,
  initialView = "shops",
}: {
  token: string;
  initialView?: "shops" | "orders";
}) {
  const [shops, setShops] = useState<Shop[]>([]);
  const [activeShop, setActiveShop] = useState<Shop | null>(null);
  const [products, setProducts] = useState<Product[]>([]);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [orders, setOrders] = useState<Order[]>([]);
  const [view, setView] = useState<"shops" | "orders">(initialView);

  const loadOrders = async () => {
    try {
      const data = await request("/Order/my-orders", {}, token);
      setOrders(data.orders ?? []);
    } catch {}
  };
  useEffect(() => {
    request("/Shop")
      .then((data) => setShops(data.shops ?? []))
      .catch(() =>
        antMessage.error("Could not load shops. Is the backend running?"),
      );
    loadOrders();
  }, []);

  const selectShop = async (shop: Shop) => {
    setActiveShop(shop);
    setCart([]);
    try {
      const data = await request(`/Product/shop/${shop.id}`);
      setProducts(data.products ?? []);
    } catch (e) {
      antMessage.error(
        e instanceof Error ? e.message : "Could not load products.",
      );
    }
  };

  const add = (product: Product) =>
    setCart((current) => {
      const found = current.find((item) => item.id === product.id);
      return found
        ? current.map((item) =>
            item.id === product.id
              ? {
                  ...item,
                  quantity: Math.min(item.quantity + 1, product.stock),
                }
              : item,
          )
        : [...current, { ...product, quantity: 1 }];
    });

  const placeOrder = async () => {
    if (!activeShop || cart.length === 0) return;
    try {
      const data = await request(
        "/Order",
        {
          method: "POST",
          body: JSON.stringify({
            shopId: activeShop.id,
            items: cart.map((item) => ({
              productId: item.id,
              quantity: item.quantity,
            })),
          }),
        },
        token,
      );
      antMessage.success(`Order placed! Your queue number is #${data.queueNumber}.`);
      setCart([]);
      loadOrders();
    } catch (e) {
      antMessage.error(e instanceof Error ? e.message : "Unable to place order.");
    }
  };

  const total = useMemo(
    () => cart.reduce((sum, item) => sum + item.price * item.quantity, 0),
    [cart],
  );

  return (
    <section className="dashboard">
      <div className="page-intro">
        <span className="eyebrow">CUSTOMER SPACE</span>
        <h1>Good local shopping is close by.</h1>
        <p>
          Choose a nearby shop, add what you need, then follow your order's
          queue status.
        </p>
      </div>
      <Tabs
        activeKey={view}
        onChange={(key) => setView(key as typeof view)}
        items={[
          { key: "shops", label: "Shops" },
          { key: "orders", label: "My orders" },
        ]}
      />
      {view === "orders" ? (
        <OrderTable orders={orders} />
      ) : !activeShop ? (
        <div className="shop-grid">
          {shops
            .filter((shop) => shop.isOpen)
            .map((shop) => (
              <Card
                key={shop.id}
                hoverable
                className="shop-card"
                onClick={() => selectShop(shop)}
              >
                <Tag>{shop.category}</Tag>
                <h2>{shop.shopName}</h2>
                <p>{shop.address}</p>
                <b>Browse products →</b>
              </Card>
            ))}
          {shops.length === 0 && <p>No shops are available yet.</p>}
        </div>
      ) : (
        <div className="shopping-layout">
          <section>
            <Button onClick={() => setActiveShop(null)}>← All shops</Button>
            <h2>{activeShop.shopName}</h2>
            <p className="muted">
              {activeShop.category} · {activeShop.address}
            </p>
            <div className="product-grid">
              {products.map((product) => (
                <Card key={product.id} className="product-card">
                  <Tag>{product.category}</Tag>
                  <h3>{product.name}</h3>
                  <p>{product.description}</p>
                  <strong>{formatMoney(product.price)}</strong>
                  <small>{product.stock} in stock</small>
                  <Button
                    type="primary"
                    disabled={product.stock === 0}
                    onClick={() => add(product)}
                  >
                    Add to cart
                  </Button>
                </Card>
              ))}
            </div>
          </section>
          <aside className="cart">
            <h2>Your cart</h2>
            {cart.length === 0 ? (
              <p>Your selected products appear here.</p>
            ) : (
              <>
                {cart.map((item) => (
                  <div className="cart-row" key={item.id}>
                    <span>
                      {item.name} × {item.quantity}
                    </span>
                    <strong>{formatMoney(item.price * item.quantity)}</strong>
                  </div>
                ))}
                <div className="cart-total">
                  <span>Total</span>
                  <strong>{formatMoney(total)}</strong>
                </div>
                <Button type="primary" block onClick={placeOrder}>
                  Place order
                </Button>
              </>
            )}
          </aside>
        </div>
      )}
    </section>
  );
}