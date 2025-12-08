import { getServerSession } from "next-auth";
import AdminDashboard from "./components/AdminDashboard";
import { redirect } from "next/navigation";

export default async function AdminPage() {
  const session = await getServerSession();

  if (!session) {
    redirect("/api/auth/signin");
  }

  return (
    <main className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-4xl mx-auto">
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mb-6">
          <h1 className="text-3xl font-bold text-gray-900">Yönetim Paneli</h1>
          <p className="text-gray-500 mt-2">
            Sistem durumunu kontrol edebilir ve veri toplama işlemlerini buradan yönetebilirsiniz.
          </p>
        </div>

        <AdminDashboard accessToken={session.accessToken as string} />
      </div>
    </main>
  );
}
