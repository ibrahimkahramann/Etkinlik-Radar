import { getServerSession } from "next-auth";
import Link from "next/link";

type Event = {
  id: string;
  name: string;
  description: string;
  eventDate: string;
  imageUrl: string;
  city: string;
};

async function getEvents() {
  const res = await fetch("http://localhost/api/events", { 
    cache: "no-store"
  });

  if (!res.ok) {
    throw new Error("Etkinlikler getirilemedi");
  }

  return res.json();
}

export default async function Home() {
  const session = await getServerSession();
  let events: Event[] = [];

  try {
    events = await getEvents();
  } catch (error) {
    console.error(error);
  }

  return (
    <main className="min-h-screen bg-gray-100 p-8">
      <header className="flex justify-between items-center mb-8 bg-white p-4 rounded-lg shadow">
        <h1 className="text-2xl font-bold text-indigo-600">Etkinlik Radar</h1>
        <div>
          {session ? (
            <div className="flex items-center gap-4">
              <span className="text-sm">Hoşgeldin, {session.user?.name}</span>
              <Link href="/api/auth/signout" className="text-red-500 hover:underline">Çıkış Yap</Link>
            </div>
          ) : (
            <Link href="/api/auth/signin" className="bg-indigo-600 text-white px-4 py-2 rounded hover:bg-indigo-700">
              Giriş Yap
            </Link>
          )}
        </div>
      </header>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {events.map((evt) => (
          <div key={evt.id} className="bg-white rounded-lg shadow overflow-hidden hover:shadow-lg transition">
            <img 
              src={evt.imageUrl} 
              alt={evt.name} 
              className="w-full h-48 object-cover"
            />
            <div className="p-4">
              <h2 className="font-bold text-xl mb-2 text-gray-800">{evt.name}</h2>
              <p className="text-gray-600 text-sm mb-2">{evt.city} - {new Date(evt.eventDate).toLocaleDateString()}</p>
              <p className="text-gray-500 text-sm line-clamp-2">{evt.description}</p>
              <button className="mt-4 w-full bg-indigo-50 text-indigo-600 py-2 rounded hover:bg-indigo-100 font-medium">
                Detayları Gör
              </button>
            </div>
          </div>
        ))}
      </div>
    </main>
  );
}
