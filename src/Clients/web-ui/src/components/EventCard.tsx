"use client";

import FollowButton from "./FollowButton";

type Event = {
  id: string;
  name: string;
  description: string;
  eventDate: string;
  imageUrl: string;
  city: string;
};

interface EventCardProps {
  event: Event;
  isFollowing: boolean;
}

export default function EventCard({ event, isFollowing }: EventCardProps) {
  return (
    <div className="bg-white rounded-lg shadow overflow-hidden hover:shadow-lg transition flex flex-col">
      <img
        src={event.imageUrl}
        alt={event.name}
        className="w-full h-48 object-cover"
      />
      <div className="p-4 flex-1 flex flex-col">
        <h2 className="font-bold text-xl mb-2 text-gray-800">{event.name}</h2>
        <div className="text-gray-600 text-sm mb-2">
          {event.city} - {new Date(event.eventDate).toLocaleDateString()}
        </div>
        <div className="text-gray-500 text-sm line-clamp-2 mb-4 flex-1">
          {event.description}
        </div>

        <div className="mt-auto">
          <button className="w-full bg-indigo-50 text-indigo-600 py-2 rounded hover:bg-indigo-100 font-medium mb-2">
            Detayları Gör
          </button>
          <FollowButton
            artistId={event.name}
            initialIsFollowing={isFollowing}
          />
        </div>
      </div>
    </div>
  );
}
