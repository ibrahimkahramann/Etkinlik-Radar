import { getServerSession } from "next-auth";
import Link from "next/link";
import FollowButton from "@/components/FollowButton";

type Event = {
  id: string;
  name: string;
  description: string;
  eventDate: string;
  imageUrl: string;
  city: string;
};

async function getEvents() {
  try {
    const res = await fetch("http://localhost/api/events", {
      cache: "no-store"
    });

    if (!res.ok) {
      console.error("Failed to fetch events:", res.status, res.statusText);
      return [];
    }

    return res.json();
  } catch (error) {
    console.error("Error fetching events:", error);
    return [];
  }
}

async function getFollowedArtists(accessToken?: string) {
  if (!accessToken) return [];

  try {
    const res = await fetch("http://localhost/api/followers", {
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
      cache: "no-store",
    });

    if (!res.ok) return [];

    const data = await res.json();
    return data.map((f: any) => f.artistId);
  } catch (error) {
    console.error("Error fetching follows:", error);
    return [];
  }
}

export default async function Home() {
  const session = await getServerSession();
  let events: Event[] = [];
  let followedArtists: string[] = [];

  try {
    events = await getEvents();
    if (session && (session as any).accessToken) {
      followedArtists = await getFollowedArtists((session as any).accessToken);
    }
  } catch (error) {
    console.error("Error in Home:", error);
  }

  if (!Array.isArray(events)) {
    events = [];
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
        {events.map((evt, index) => (
          <div key={`${evt.id}-${index}`} className="bg-white rounded-lg shadow overflow-hidden hover:shadow-lg transition flex flex-col">
            <img
              src={evt.imageUrl}
              alt={evt.name}
              className="w-full h-48 object-cover"
            />
            <div className="p-4 flex-1 flex flex-col">
              <h2 className="font-bold text-xl mb-2 text-gray-800">{evt.name}</h2>
              <div className="text-gray-600 text-sm mb-2" suppressHydrationWarning>{evt.city} - {new Date(evt.eventDate).toLocaleDateString()}</div>
              <div className="text-gray-500 text-sm line-clamp-2 mb-4 flex-1">{evt.description}</div>

              <div className="mt-auto">
                <button className="w-full bg-indigo-50 text-indigo-600 py-2 rounded hover:bg-indigo-100 font-medium mb-2">
                  Detayları Gör
                </button>
                <FollowButton
                  artistId={evt.name}
                  initialIsFollowing={followedArtists.includes(evt.name)}
                />
              </div>
            </div>
          </div>
        ))}
      </div>
    </main>
  );
}
