import { Component, Input, OnChanges, OnDestroy, AfterViewInit, SimpleChanges, ElementRef, ViewChild } from '@angular/core';
import * as L from 'leaflet';


delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

@Component({
  selector: 'app-map',
  standalone: true,
  templateUrl: './map.html'
})
export class MapComponent implements AfterViewInit, OnChanges, OnDestroy {
  @Input() latitude: number | null = null;
  @Input() longitude: number | null = null;
  @Input() zoom: number = 15;
  @Input() popupText: string = '';

  @ViewChild('mapContainer', { static: true }) mapContainer!: ElementRef<HTMLDivElement>;

  private map: L.Map | null = null;
  private marker: L.Marker | null = null;
  private viewInitialized = false;

  ngAfterViewInit(): void {
    this.viewInitialized = true;
    this.initMap();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.viewInitialized) return;

    if (this.map) {
      this.updateMap();
    } else {
      this.initMap();
    }
  }

  private initMap(): void {
    if (this.latitude == null || this.longitude == null) return;

    this.map = L.map(this.mapContainer.nativeElement, {
      center: [this.latitude, this.longitude],
      zoom: this.zoom
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);

    this.marker = L.marker([this.latitude, this.longitude]).addTo(this.map);
    if (this.popupText) {
      this.marker.bindPopup(this.popupText).openPopup();
    }
  }

  private updateMap(): void {
    if (!this.map || this.latitude == null || this.longitude == null) return;
    const newLatLng: L.LatLngExpression = [this.latitude, this.longitude];
    this.map.setView(newLatLng, this.zoom);
    this.marker?.setLatLng(newLatLng);
    if (this.popupText) {
      this.marker?.bindPopup(this.popupText);
    }
  }

  ngOnDestroy(): void {
    this.map?.remove();
    this.map = null;
  }
}