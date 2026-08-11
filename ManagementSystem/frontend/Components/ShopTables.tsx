"use client";

import { Table, Tag, Select, Empty } from "antd";

export type Product = {
  id: string;
  name: string;
  description: string;
  price: number;
  stock: number;
  category: string;
  isAvailable: boolean;
};

export type Order = {
  id: string;
  customerId: string;
  items: { productName: string; quantity: number; price: number }[];
  totalAmount: number;
  queueNumber: number;
  status: string;
  createdAt: string;
  estimatedReadyInMinutes: number;
};

const formatMoney = (amount: number) =>
  new Intl.NumberFormat("en-IN", { style: "currency", currency: "INR" }).format(
    amount,
  );

const STATUS_OPTIONS = ["Pending", "Preparing", "Ready", "Completed", "Cancelled"];

const STATUS_COLORS: Record<string, string> = {
  Pending: "gold",
  Preparing: "blue",
  Ready: "green",
  Completed: "cyan",
  Cancelled: "red",
};

export function ProductTable({ products }: { products: Product[] }) {
  const columns = [
    {
      title: "Product",
      dataIndex: "name",
      key: "name",
      render: (_: string, record: Product) => (
        <div>
          <div style={{ fontWeight: 600 }}>{record.name}</div>
          <div style={{ fontSize: 12, color: "var(--ant-color-text-secondary, #888)" }}>
            {record.description}
          </div>
        </div>
      ),
    },
    { title: "Category", dataIndex: "category", key: "category" },
    {
      title: "Price",
      dataIndex: "price",
      key: "price",
      render: (price: number) => formatMoney(price),
    },
    { title: "Stock", dataIndex: "stock", key: "stock" },
    {
      title: "Availability",
      dataIndex: "isAvailable",
      key: "isAvailable",
      render: (isAvailable: boolean) => (
        <Tag color={isAvailable ? "green" : "default"}>
          {isAvailable ? "Available" : "Hidden"}
        </Tag>
      ),
    },
  ];

  return (
    <section className="table-wrap">
      <div className="table-heading">
        <h2>Inventory</h2>
        <span>{products.length} products</span>
      </div>
      <Table
        rowKey="id"
        columns={columns}
        dataSource={products}
        pagination={{ pageSize: 8, hideOnSinglePage: true }}
        locale={{
          emptyText: (
            <Empty description="No products yet. Add your first product above." />
          ),
        }}
      />
    </section>
  );
}

export function OrderTable({
  orders,
  allowUpdate,
  onUpdate,
}: {
  orders: Order[];
  allowUpdate?: boolean;
  onUpdate?: (id: string, status: string) => void;
}) {
  const columns = [
    {
      title: "Queue",
      dataIndex: "queueNumber",
      key: "queueNumber",
      render: (queueNumber: number) => <strong>#{queueNumber}</strong>,
    },
    {
      title: "Items",
      dataIndex: "items",
      key: "items",
      render: (items: Order["items"]) =>
        items.map((item) => `${item.productName} × ${item.quantity}`).join(", "),
    },
    {
      title: "Total",
      dataIndex: "totalAmount",
      key: "totalAmount",
      render: (amount: number) => formatMoney(amount),
    },
    {
      title: "Status",
      dataIndex: "status",
      key: "status",
      render: (status: string) => (
        <Tag color={STATUS_COLORS[status] ?? "default"}>{status}</Tag>
      ),
    },
    {
      title: allowUpdate ? "Update" : "ETA",
      key: "action",
      render: (_: unknown, record: Order) =>
        allowUpdate ? (
          <Select
            value={record.status}
            style={{ width: 140 }}
            onChange={(value) => onUpdate?.(record.id, value)}
            options={STATUS_OPTIONS.map((status) => ({
              value: status,
              label: status,
            }))}
          />
        ) : record.estimatedReadyInMinutes ? (
          `${record.estimatedReadyInMinutes} min`
        ) : (
          "—"
        ),
    },
  ];

  return (
    <section className="table-wrap">
      <div className="table-heading">
        <h2>{allowUpdate ? "Incoming orders" : "My orders"}</h2>
        <span>{orders.length} orders</span>
      </div>
      <Table
        rowKey="id"
        columns={columns}
        dataSource={orders}
        pagination={{ pageSize: 8, hideOnSinglePage: true }}
        locale={{
          emptyText: <Empty description="No orders to show yet." />,
        }}
      />
    </section>
  );
}