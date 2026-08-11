"use client";

import { ChangeEvent, useEffect, useState } from "react";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5115/api";

export default function ShopkeeperExcelUploadPage() {
  const [token, setToken] = useState<string | null>(null);
  const [file, setFile] = useState<File | null>(null);
  const [message, setMessage] = useState("");

  useEffect(() => {
    const saved = localStorage.getItem("local-shop-session");
    const session = saved ? JSON.parse(saved) : null;
    if (!session || session.role !== "Shopkeeper") {
      window.location.assign("/");
      return;
    }
    setToken(session.token);
  }, []);

  const chooseFile = (event: ChangeEvent<HTMLInputElement>) => {
    setFile(event.target.files?.[0] ?? null);
    setMessage("");
  };

  const upload = async () => {
    if (!file || !token) return;
    const body = new FormData();
    body.append("file", file);
    try {
      const response = await fetch(`${API_URL}/Product/upload-excel`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
        body,
      });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message ?? "Upload failed.");
      setMessage(data.message);
      setFile(null);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Upload failed.");
    }
  };

  const signOut = () => {
    localStorage.removeItem("local-shop-session");
    window.location.assign("/");
  };

  if (!token)
    return <main className="route-loading">Opening Excel upload…</main>;

  return (
    <main className="app-shell">
      <header>
        <a className="brand" href="/shopkeeper">
          Neighbourhood Hub
        </a>
        <nav className="route-nav">
          <a href="/shopkeeper">Dashboard</a>
          <a className="active" href="/shopkeeper/excel-upload">
            Excel upload
          </a>
        </nav>
        <span className="role-pill">Shopkeeper</span>
        <button className="text-button" onClick={signOut}>
          Sign out
        </button>
      </header>
      <section className="dashboard">
        <div className="page-intro">
          <span className="eyebrow">BULK INVENTORY IMPORT</span>
          <h1>Upload products from Excel.</h1>
          <p>
            Import your full inventory in one step. Your shop must be created
            first.
          </p>
        </div>
        <section className="panel upload-panel">
          <h2>Choose an Excel file</h2>
          <p className="muted">
            Only <strong>.xlsx</strong> files are accepted. Use this exact
            first row: <code>Name, Description, Price, Stock, Category</code>.
          </p>
          <input
            type="file"
            accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            onChange={chooseFile}
          />
          {file && (
            <p>
              <strong>Selected:</strong> {file.name}
            </p>
          )}
          {message && <p className="notice">{message}</p>}
          <button className="primary" disabled={!file} onClick={upload}>
            Upload products
          </button>
        </section>
      </section>
    </main>
  );
}