"use client";

import { useEffect, useState } from "react";
import {
  AdminPanel,
  CustomerPanel,
  Role,
  ShopkeeperPanel,
} from "./LocalShopManagement";

export default function RolePage({
  role,
  shopkeeperTab,
  customerView,
}: {
  role: Role;
  shopkeeperTab?: "inventory" | "orders" | "shop";
  customerView?: "shops" | "orders";
}) {
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    const saved = localStorage.getItem("local-shop-session");
    const session = saved ? JSON.parse(saved) : null;
    if (!session || session.role !== role) {
      window.location.assign("/");
      return;
    }
    setToken(session.token);
  }, [role]);

  const signOut = () => {
    localStorage.removeItem("local-shop-session");
    window.location.assign("/");
  };

  if (!token)
    return <main className="route-loading">Opening your dashboard…</main>;

  return (
    <main className="app-shell">
      <header>
        <a className="brand" href={`/${role.toLowerCase()}`}>
          Neighbourhood Hub
        </a>
        <nav className="route-nav">
          {role === "Admin" && <a href="/admin">Admin dashboard</a>}
          {role === "Shopkeeper" && (
            <>
              <a href="/shopkeeper">Inventory</a>
              <a href="/shopkeeper/orders">Orders</a>
              <a href="/shopkeeper/shop">My shop</a>
              <a href="/shopkeeper/excel-upload">Excel upload</a>
            </>
          )}
          {role === "Customer" && (
            <>
              <a href="/customer">Browse shops</a>
              <a href="/customer/orders">My orders</a>
            </>
          )}
        </nav>
        <span className="role-pill">{role}</span>
        <button className="text-button" onClick={signOut}>
          Sign out
        </button>
      </header>
      {role === "Admin" ? (
        <AdminPanel token={token} />
      ) : role === "Shopkeeper" ? (
        <ShopkeeperPanel token={token} initialTab={shopkeeperTab} />
      ) : (
        <CustomerPanel token={token} initialView={customerView} />
      )}
    </main>
  );
}